dotnet publish ../src/back/gateway/Doctor.Management.Gateway --os linux --arch x64 -c Release

dotnet publish ../src/back/services/auth//Services.Auth.Api --os linux --arch x64 -c Release
dotnet publish ../src/back/services/diagnosis/Service.Diagnoses.Api --os linux --arch x64 -c Release
dotnet publish ../src/back/services/doctor/Services.Doctors.Api --os linux --arch x64 -c Release
dotnet publish ../src/back/services/patient/Services.Patient.Api --os linux --arch x64 -c Release