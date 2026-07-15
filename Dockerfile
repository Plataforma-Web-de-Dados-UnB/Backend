FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev

WORKDIR /src

RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 5042

COPY ["Backend/api.csproj", "Backend/"]
RUN dotnet restore "Backend/api.csproj"

COPY . .

WORKDIR "/src/Backend/"

CMD ["dotnet", "run", "--urls", "http://0.0.0.0:5042"]

FROM dev AS test
CMD ["sh", "-c", "dotnet test --list-tests && dotnet test -l \"console;verbosity=normal\" -- RunConfiguration.TreatNoTestsAsError=true"]

FROM dev AS lint
CMD ["dotnet", "format", "api.csproj", "--verify-no-changes", "--verbosity", "d"]
