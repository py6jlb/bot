# bot

# Deploy

## Certificates

Для деплоя сервиса gRPC нужно сгенерировать сертификат:
```
openssl genrsa 2048 > private.pem
openssl req -x509 -new -key private.pem -out public.pem
openssl pkcs12 -export -in public.pem -inkey private.pem -out mycert.pfx
```

## Docker-compose

Для зупуска надо создать файл dokcer-compose.yml с примерно следующим содержанием:
```yml
version: '3.4' 

networks: 
  app: 
    driver: "bridge" 
 
services: 
  bot.datastore: 
    image: py6jlb/bot_store:latest 
    environment: 
      - ASPNETCORE_ENVIRONMENT=Production 
      - ASPNETCORE_URLS=https://+:443;http://+:80 
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/root/.aspnet/https/mycert.pfx 
      - ASPNETCORE_Kestrel__Certificates__Default__Password=123456 
    ports: 
      - "80" 
      - "443" 
    volumes: 
      - ./https:/root/.aspnet/https:ro 
      - ./db:/app/db 
    networks: 
      app: 
        aliases: 
          - "datastore" 
    restart: always 
 
  bot.telegramworker: 
    image: py6jlb/bot:latest 
    environment: 
      - DOTNET_ENVIRONMENT=Production 
      - DOTNET_BOT_API_KEY= code 
      - DOTNET_ALLOWED_USERS= username||username||username 
      - DOTNET_BOT_DATA_STORE_ENDPOINT=https://datastore 
    networks: 
      app: 
        aliases: 
          - "bot_worker" 
    depends_on: 
      - "bot.datastore" 
    restart: always 
```

