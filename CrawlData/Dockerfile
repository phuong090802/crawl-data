FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /App
COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /CrawlData
COPY --from=build-env /App/appsettings.json . 
COPY --from=build-env /App/out .

ARG CHROME_VERSION="117.0.5938.62-1"
RUN apt-get update && apt-get -f install && apt-get -y install wget gnupg2 apt-utils
RUN wget --no-verbose -O /tmp/chrome.deb https://dl.google.com/linux/chrome/deb/pool/main/g/google-chrome-stable/google-chrome-stable_${CHROME_VERSION}_amd64.deb \
    && apt-get update \
    && apt-get install -y /tmp/chrome.deb --no-install-recommends --allow-downgrades fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf \
    && rm /tmp/chrome.deb

# create system group 'pptruser'
# create system user 'pptruser' in group 'audio', 'video' and 'pptruser'
# make dir 'Downloads' in folder '/home/pptruser'
# set onwner '/home/pptruser'
RUN groupadd -r pptruser && useradd -r -g pptruser -G audio,video pptruser \
    && mkdir -p /home/pptruser/Downloads \
    && chown -R pptruser:pptruser /home/pptruser

USER pptruser

ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/google-chrome-stable"


ENTRYPOINT ["dotnet", "CrawlData.dll"]
