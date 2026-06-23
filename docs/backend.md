# Kartkatalog

## Beskrivelse

Api for søk/visning av metadata som er registrert i GeoNetwork. Se: https://kartkatalog.geonorge.no/swagger/index.html

## Kjøremiljø

**Dev:** [http://kartkatalog.dev.geonorge.no/](http://kartkatalog.dev.geonorge.no/ "http://kartkatalog.dev.geonorge.no/")

**Test:** [https://kartkatalog.test.geonorge.no/](https://kartkatalog.test.geonorge.no/ "https://kartkatalog.test.geonorge.no/")

**Prod:** [https://kartkatalog.geonorge.no/](https://kartkatalog.geonorge.no/ "https://kartkatalog.geonorge.no/")

## Sitemap:

[https://kartkatalog.geonorge.no/api/sitemap](https://kartkatalog.geonorge.no/api/sitemap "https://kartkatalog.geonorge.no/api/sitemap")

**Frontend**

Razor og Vue.js frontend for visning av nedlastings-kurv, se: https://github.com/kartverket/Geonorge.Kartkatalog/blob/main/Kartverket.Metadatakatalog/Views/Download/Index.cshtml

For å laste ned pakker, kjør: yarn

**Backend**

Backend leverer i hovedsak api-kall til fornt-end, samt nedlastings-kurv.  
[http://kartkatalog-backend.dev.geonorge.no](http://kartkatalog-backend.dev.geonorge.no "http://kartkatalog-backend.dev.geonorge.no")

Proxy

Det ligger en nginx-proxy som leverer [https://kartkatalog.dev.geonorge.no](https://kartkatalog.dev.geonorge.no/ "https://kartkatalog.dev.geonorge.no/")

Følgende urler leveres fra backed-applikasjonen:

* /api
* /nedlasting
* /dist
* /Content
* /Scripts
  

Alle andre urler blir sendt videre til frontend-applikasjonen som leverer brukergrensesnittet til brukeren.

## Teknisk

**Kildekode:** [https://github.com/kartverket/Geonorge.Kartkatalog](https://github.com/kartverket/Geonorge.Kartkatalog "https://github.com/kartverket/Geonorge.Kartkatalog")

Applikasjonen er utviklet med C# og .NET 10.


## Utviklingsmiljø

### .env/appsettings.local.json

Se i appsettings.json-filene for miljø-variabler. 

Avhengig av Solr index kjører.

## Solr

Skal kjøres på samme maskin som kartkatalogen. Installasjonen for Solr, med tilhørende skjema for indeksering, ligger i prosjektet **Kartverket.Metadatakatalog.Solr** på [https://github.com/kartverket/Geonorge.Kartkatalog.Solr](https://github.com/kartverket/Geonorge.Kartkatalog.Solr "https://github.com/kartverket/Geonorge.Kartkatalog.Solr")
Også oppsett for å kjøre som container.

Java Runtime Environment må være installert på maskinen. Solr startes fra kommandolinje med **java -jar start.jar --module=http** 

Solr kan benyttes med med dense vector search. Det hentes vector embeddings fra Gemini Enterprise Agent Platform API ([console.cloud.google.com](http://console.cloud.google.com "http://console.cloud.google.com")), 

## Indeksering

Full indeksering startes ved å kjøre /api/index-metadata (eventuuelt dersom man først vil slette alt /api/reindex-metadata). For oppdatering av en oppføring, se: /api/metadataupdated

GeoNetwork er satt opp til å gi beskjed til /api/metadataupdated når metadata endres.