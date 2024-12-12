using Services.Doctors.Domain.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Doctors.Domain.Entities;
using Services.Doctors.Persistence.Context;
using Value.Objects.Helper.Values.Primitives;
using System.Linq.Expressions;

namespace Services.Doctors.Persistence.Repositories;

internal class DoctorCompiledQueries
{
    private static readonly int _pageSize = 10;

    protected static readonly Func<AppDbContext, Expression<Func<Doctor, bool>>, Task<bool>> AnyFilter =
        EF.CompileAsyncQuery(
            (AppDbContext context, Expression<Func<Doctor, bool>> filter)
                => context.Set<Doctor>().AsNoTracking().Any(filter));

    protected static readonly Func<AppDbContext, string, int, IAsyncEnumerable<DoctorDto>> GetCollectionByName =
        EF.CompileAsyncQuery(
            (AppDbContext context, string name, int pageNumber)
                => context.Set<Doctor>()
                    .Where(w => w.NormalizedName.Contains(name))
                    .OrderBy(w => w.Name)
                    .Select(s => DoctorDto.Create(
                        s.Id,
                        s.Name,
                        s.Specialty,
                        s.ExperienceInYears,
                        s.AuditDates))
                    .Skip((pageNumber - 1) * _pageSize)
                    .Take(_pageSize));

    protected static readonly Func<AppDbContext, StringObject, int, IAsyncEnumerable<DoctorDto>> GetCollectionBySpecialty =
        EF.CompileAsyncQuery(
            (AppDbContext context, StringObject specialty, int pageNumber)
                => context.Set<Doctor>()
                    .Where(w => w.Specialty.Equals(specialty))
                    .OrderBy(w => w.Name)
                    .Select(s => DoctorDto.Create(
                        s.Id,
                        s.Name,
                        s.Specialty,
                        s.ExperienceInYears,
                        s.AuditDates))
                    .Skip((pageNumber - 1) * _pageSize)
                    .Take(_pageSize));

    protected static readonly Func<AppDbContext, IAsyncEnumerable<StringObject>> GetSpecialtyCollection =
        EF.CompileAsyncQuery(
            (AppDbContext context)
                => context.Set<Doctor>().AsNoTracking()
                    .Select(w => w.Specialty)
                    .Distinct()
                    .OrderBy(w => w)
                    .AsQueryable());
}
