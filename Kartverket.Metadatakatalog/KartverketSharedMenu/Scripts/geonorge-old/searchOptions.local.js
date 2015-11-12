//DO NOT ALTER - SYNCED WITH Sections in server side code
var sections = {
    Everything: 'Søk i alt',
    EverythingID: 0,
    Geonorge: 'Geonorge',
    GeonorgeID: 1,
    Metadata: 'Kartkatalogen',
    MetadataID: 2,
    ObjectCatalogue: 'Objektkatalogen',
    ObjectCatalogueID: 3
};

var dropdownOptions = [
              {
                  text: "Søk i alt",
                  searchTitle: "Geonorge",
                  buttonCss: "edgesGeonorge",
                  listCss: "left-edge-geonorge",
                  url: "http://localhost:50306/searchall",
                  /*url: "http://kartverket.steria.dev/Geonorge/Geonorge-sok",*/
                  queryParameter: '?text=',
                  localUrl: true,
                  autoComplete: true,
                  section: sections.EverythingID
              },
              {
                  text: "Artikler og dokumenter",
                  searchTitle: "Geonorge",
                  buttonCss: "edgesArtikler",
                  listCss: "left-edge-artikler",
                  url: "http://localhost:50306/searchall",
                  /*url: "http://kartverket.steria.dev/Geonorge/Geonorge-sok",*/
                  queryParameter: '?text=',
                  localUrl: true,
                  autoComplete: true,
                  section: sections.GeonorgeID
              },
              {
                  text: "Kartkatalogen",
                  searchTitle: "Kartkatalogen",
                  buttonCss: "edgesKartkatalogen",
                  listCss: "left-edge-kartkatalogen",
                  url: "http://kartkatalog.dev.geonorge.no/search",
                  queryParameter: '?text=',
                  localUrl: false,
                  autoComplete: true,
                  section: sections.MetadataID
              },
            {
                text: "Norgeskart",
                searchTitle: "Norgeskart",
                buttonCss: "edgesNorgeskart",
                listCss: "left-edge-norgeskart",
                url: "http://www.norgeskart.no/geoportal",
                queryParameter: '?sok=',
                localUrl: false
            },
           {
               text: "Registre",
               searchTitle: "Registre",
               buttonCss: "edgesRegistre",
               listCss: "left-edge-registre",
               url: "http://register.dev.geonorge.no/",
               queryParameter: '?text=',
               localUrl: false
           }
];