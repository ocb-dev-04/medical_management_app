dotnet ef dbcontext optimize --context AppDbContext --output-dir CompiledEntities --project ../../src/services/auth/Services.Auth.Persistence --startup-project ../../src/services/auth/Services.Auth.Api
dotnet ef dbcontext optimize --context AppDbContext --output-dir CompiledEntities --project ../../src/services/doctor/Services.Doctors.Persistence --startup-project ../../src/services/doctor/Services.Doctors.Api