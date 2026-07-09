# Покладіть цей файл в КОРІНЬ репозиторію (поруч з папками CourseManagementApi і SmartTable.Client)

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копіюємо обидва проєкти (client потрібен через ProjectReference в API)
COPY SmartTable.Client/ SmartTable.Client/
COPY CourseManagementApi/ CourseManagementApi/

WORKDIR /src/CourseManagementApi
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "CourseManagementApi.dll"]
