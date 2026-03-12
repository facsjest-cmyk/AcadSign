using AcadSign.Backend.Application.Common.Interfaces;
using AcadSign.Backend.Domain.Entities;
using AcadSign.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcadSign.Backend.Infrastructure.Data.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Students.FirstOrDefaultAsync(s => s.PublicId == id, cancellationToken);
    }

    public async Task<Student?> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default)
    {
        if (Guid.TryParse(studentId, out var guid))
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.PublicId == guid, cancellationToken);
        }

        if (int.TryParse(studentId, out var intId))
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Id == intId, cancellationToken);
        }

        return null;
    }

    public async Task<List<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students.ToListAsync(cancellationToken);
    }

    public async Task<Student> CreateAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return student;
    }

    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            return;
        }

        _context.Students.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
