#imagen para compilar nuestra aplicacion--compila desde el primer from hasta el segundo
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#creame la carpeta App y metemelo dentro y trabajo con esa carpeta
WORKDIR /App

# copia el contenido que este en la misma altura que Dokerfile en el microcontenedor App
COPY . ./
#Restaura y guarda la cache
RUN dotnet restore

# copy and publish app and libraries
#comando para publicar --dentro del contenedor---contruye y publica y compilame y me lo mandas a la carpeta out
RUN dotnet publish -c Release -o out

# SERVIDOR WEB
#Bajamos la imagen y la ejecuta
FROM mcr.microsoft.com/dotnet/aspnet:6.0
#Cre la carpeta app en el directorio de trabajo
WORKDIR /App
#Copia de mi ordenador a la imagen de la capa build en app/out que es el dotnet publish que hemos realizado
COPY --from=build /App/out .
#ejecuta el nombre.dll 
ENTRYPOINT ["dotnet","Emotions.dll"]



