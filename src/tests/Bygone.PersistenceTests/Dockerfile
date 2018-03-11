FROM microsoft/dotnet:2.0-sdk-jessie AS build

COPY . ./app
WORKDIR /app/src/tests/Bygone.PersistenceTests
RUN curl https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh > /app/wait_for_it.sh \
	&& dotnet restore
