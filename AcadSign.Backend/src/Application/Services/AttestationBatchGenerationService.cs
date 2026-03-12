using System.Security.Cryptography;
using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Application.Common.Models;
using AcadSign.Backend.Application.Interfaces;
using AcadSign.Backend.Application.Models;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AcadSign.Backend.Application.Services;

public class AttestationBatchGenerationService : IAttestationBatchGenerationService
{
    private readonly ISisAttestationExportClient _sisClient;
    private readonly IPdfGenerationService _pdfService;
    private readonly IS3StorageService _s3Storage;
    private readonly IDocumentRepository _documentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<AttestationBatchGenerationService> _logger;

    public AttestationBatchGenerationService(
        ISisAttestationExportClient sisClient,
        IPdfGenerationService pdfService,
        IS3StorageService s3Storage,
        IDocumentRepository documentRepository,
        IStudentRepository studentRepository,
        ILogger<AttestationBatchGenerationService> logger)
    {
        _sisClient = sisClient;
        _pdfService = pdfService;
        _s3Storage = s3Storage;
        _documentRepository = documentRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<SisAttestationBatchGenerationResult> GenerateFromSisAsync(
        DocumentType documentType,
        CancellationToken cancellationToken = default)
    {
        var sisResult = await _sisClient.GetStudentsAsync(cancellationToken);

        var result = new SisAttestationBatchGenerationResult
        {
            DocumentType = documentType,
            Total = sisResult.Items.Count + sisResult.Errors.Count
        };

        foreach (var error in sisResult.Errors)
        {
            result.Failures.Add(new SisAttestationBatchFailure
            {
                ItemIndex = error.ItemIndex,
                Apogee = error.Apogee,
                Nom = error.Nom,
                Prenom = error.Prenom,
                Filiere = error.Filiere,
                Code = error.Code,
                Message = error.Message
            });
        }

        foreach (var student in sisResult.Items)
        {
            try
            {
                var documentId = Guid.NewGuid();

                var studentPublicId = await EnsureStudentAsync(student, cancellationToken);

                var studentData = MapSisToStudentData(student, documentId);

                var pdfBytes = await _pdfService.GenerateDocumentAsync(documentType, studentData);

                var s3ObjectPath = await _s3Storage.UploadDocumentAsync(pdfBytes, documentId.ToString());

                var document = new Document
                {
                    PublicId = documentId,
                    DocumentType = documentType.ToString(),
                    StudentId = studentPublicId,
                    Status = "UNSIGNED",
                    S3ObjectPath = s3ObjectPath
                };

                await _documentRepository.CreateAsync(document, cancellationToken);

                result.Generated += 1;
                result.CreatedDocumentIds.Add(documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed generating document from SIS item apogee={Apogee}", student.Apogee);

                result.Failures.Add(new SisAttestationBatchFailure
                {
                    Apogee = student.Apogee,
                    Nom = student.Nom,
                    Prenom = student.Prenom,
                    Filiere = student.Filiere,
                    Code = "GENERATION_FAILED",
                    Message = ex.Message
                });
            }
        }

        result.Failed = result.Failures.Count;
        return result;
    }

    private async Task<Guid> EnsureStudentAsync(SisAttestationStudentDto dto, CancellationToken cancellationToken)
    {
        var publicId = DeriveStableStudentGuidFromApogee(dto.Apogee ?? string.Empty);
        if (publicId == Guid.Empty)
        {
            return Guid.Empty;
        }

        var existing = await _studentRepository.GetByIdAsync(publicId, cancellationToken);
        if (existing != null)
        {
            existing.FirstName = dto.Prenom ?? existing.FirstName;
            existing.LastName = dto.Nom ?? existing.LastName;
            existing.CNE = dto.Apogee ?? existing.CNE;
            await _studentRepository.UpdateAsync(existing, cancellationToken);
            return existing.PublicId;
        }

        var student = new Student
        {
            PublicId = publicId,
            FirstName = dto.Prenom ?? string.Empty,
            LastName = dto.Nom ?? string.Empty,
            CNE = dto.Apogee ?? string.Empty,
            CIN = string.Empty,
            Email = string.Empty,
            PhoneNumber = null,
            DateOfBirth = DateTime.UnixEpoch,
            InstitutionId = Guid.Empty
        };

        await _studentRepository.CreateAsync(student, cancellationToken);
        return student.PublicId;
    }

    private static StudentData MapSisToStudentData(SisAttestationStudentDto dto, Guid documentId)
    {
        return new StudentData
        {
            DocumentId = documentId,

            FirstNameFr = dto.Prenom ?? string.Empty,
            LastNameFr = dto.Nom ?? string.Empty,

            FirstNameAr = string.Empty,
            LastNameAr = string.Empty,

            ProgramNameFr = dto.Filiere ?? string.Empty,
            ProgramNameAr = string.Empty,

            FacultyFr = string.Empty,
            FacultyAr = string.Empty,

            CIN = string.Empty,
            CNE = dto.Apogee ?? string.Empty,

            DateOfBirth = DateTime.UnixEpoch,
            AcademicYear = string.Empty,

            EnrollmentDate = DateTime.UnixEpoch,
            EnrollmentStatus = string.Empty
        };
    }

    private static Guid DeriveStableStudentGuidFromApogee(string apogee)
    {
        if (string.IsNullOrWhiteSpace(apogee))
        {
            return Guid.Empty;
        }

        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"SIS:APOGEE:{apogee.Trim()}"));
        var guidBytes = new byte[16];
        Array.Copy(bytes, guidBytes, 16);

        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}
