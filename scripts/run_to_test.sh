dotnet run --project ../src/back/gateway/Doctor.Management.Gateway -c Development

dotnet run --project ../src/back/services/auth//Services.Auth.Api -c Development
dotnet run --project ../src/back/services/diagnosis/Service.Diagnoses.Api -c Development
dotnet run --project ../src/back/services/doctor/Services.Doctors.Api -c Development
dotnet run --project ../src/back/services/patient/Services.Patient.Api -c Development