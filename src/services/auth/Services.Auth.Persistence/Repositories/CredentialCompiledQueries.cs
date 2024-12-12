using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Services.Auth.Domain.Entities;
using Services.Auth.Persistence.Context;
using Value.Objects.Helper.Values.Domain;

namespace Services.Auth.Persistence.Repositories;

internal class CredentialCompiledQueries
{
    protected static readonly Func<AppDbContext, Expression<Func<Credential, bool>>, Task<bool>> AnyFilter =
        EF.CompileAsyncQuery(
            (AppDbContext context, Expression<Func<Credential, bool>> filter)
                => context.Set<Credential>().AsNoTracking().Any(filter));

    protected static readonly Func<AppDbContext, EmailAddress, Task<Credential?>> GetCredentialByEmail =
        EF.CompileAsyncQuery(
            (AppDbContext context, EmailAddress email) =>
                context.Set<Credential>().FirstOrDefault(c => c.Email.Equals(email)));
}
