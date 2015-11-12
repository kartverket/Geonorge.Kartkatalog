var map;
var geoCoder;
var myApp = angular.module('searchApp', []);

myApp.controller('SearchController', function ($scope, $filter, $timeout) {
    $scope.countyList = countyList;
    $scope.storedSearches = [];
    $scope.resultList = [];
    $scope.showGeoLocation = false;
    $scope.searchActive = true;
    $scope.mapInitialized = false;
    $scope.displayGeoLocation = function () {
        $scope.showGeoLocation == true ? $scope.showGeoLocation = false : $scope.showGeoLocation = true;
        if ($scope.showGeoLocation) {
            if (!$scope.mapInitialized) {
                initializeMap();
                $scope.mapInitialized = true;
            }

        }
    };

    $scope.selectResultType = function () {
        console.log($scope.resultType);
    };

    $scope.Search = function () {

        $scope.storedSearches.push({
            resultType: $scope.resultType,
            query: $scope.searchQuery
        });

        $scope.resultList = [];
        var result = $filter('filter')(mockData, $scope.searchQuery);
        $scope.resultList = $scope.resultList.concat(result);

        $scope.showGeoLocation = false;
        $scope.searchActive = false;
        $scope.searchStyle = { 'margin-top': '-73px' };
    };

    $scope.setCountyBounds = function () {
        console.log($scope.county);
        geoCoder.geocode({ 'address': $scope.county }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {

                map.fitBounds(results[0].geometry.bounds);
                map.setZoom(7);
            }
        });
    };

    $scope.focusSearch = function () {

        $scope.searchStyle = {};
        $timeout(function () {
            $scope.searchActive = true;
        }, 500);
    };
});




function initializeMap() {

    geoCoder = new google.maps.Geocoder();

    var mapOptions = {
        zoom: 3,
        center: new google.maps.LatLng(63.430515, 10.395053)
    };
    map = new google.maps.Map(document.getElementById('map-canvas'),
        mapOptions);
    var drawingManager = new google.maps.drawing.DrawingManager({
        drawingMode: google.maps.drawing.OverlayType.POLYGON,
        drawingControl: true,
        drawingControlOptions: {
            position: google.maps.ControlPosition.TOP_CENTER,
            drawingModes: [
              google.maps.drawing.OverlayType.CIRCLE,
              google.maps.drawing.OverlayType.POLYGON,
              google.maps.drawing.OverlayType.RECTANGLE
            ]
        },
        markerOptions: {
            icon: 'images/beachflag.png'
        },
        circleOptions: {
            fillColor: '#ffff00',
            fillOpacity: 1,
            strokeWeight: 5,
            clickable: false,
            editable: true,
            zIndex: 1
        },
        polygonOptions: {
            draggable: true,
            editable: true
        }
    });
    drawingManager.setMap(map);

    google.maps.event.addListener(drawingManager, 'polygoncomplete', function (polygon) {
        console.log(polygon.getPaths().getArray()[0].j);

        polygon.addListener('dragend', function(e) {
            var len1 = polygon.getPaths().getLength();
            var len2 = polygon.getPath().getLength();
            console.log('--------------getPaths()----------------');
            for (var i = 0; i < len1; i++) {
                console.log(polygon.getPaths().getAt(i));
            }

            console.log('--------------getPath()----------------');
            for (var x = 0; x < len2; x++) {
                console.log(polygon.getPath().getAt(x));
            }
        });

        polygon.addListener('click', function() {
            this.setMap(null);
        });
    });

    google.maps.event.addListener(drawingManager, 'circlecomplete', function (circle) {
        console.log(circle.getBounds());

        map.setCenter(circle.getCenter());
    });
}



var mockData = [

{
    id: 1,
    title: 'Vesterålskartet',
    owner: 'Kartsamarbeidet i Vesterålen',
    typeId: 1,
    type: 'Applikasjoner',
    keyWords: ['Kommuneplanens arealdel', 'Kommuneplan', 'Arealplan', 'Reguleringsplan', 'Utbyggingsplan', 'Flybilde', 'Ortofoto']
},
{
    id: 2,
    title: 'Webplanløsning Nøtterøy',
    owner: 'Teknisk etat',
    typeId: 1,
    type: 'Applikasjoner',
    keyWords: ['webplan', 'plandialog', 'Nøtterøy', 'Vestfold']
},
{
    id: 3,
    title: 'Mineralressurser',
    owner: 'Norges geologiske undersøkelse',
    typeId: 2,
    type: 'Datasett',
    keyWords: ['prospektering', 'mineralske ressurser', 'mineraler', 'industrimineraler', 'malmer', 'metaller', 'naturstein', 'bergrettigheter', 'NGU', 'ND_DS']
},
{
    id: 4,
    title: 'Avrenning fra skyte- og øvingsfelt',
    owner: 'Forsvarsbygg Futura Miljø',
    typeId: 2,
    type: 'Datasett',
    keyWords: ['Overvåkning', 'avrenning', 'skyte- og øvingsfelt', 'Forsvarsbygg', 'vannprøver']
},
{
    id: 5,
    title: 'Sjøisdrift',
    owner: 'Meteorologisk institutt',
    typeId: 3,
    type: 'Karttjenester',
    keyWords: ['Annet', 'barentswatch']
},
{
    id: 6,
    title: 'Strømrening og fart (20km prognose)',
    owner: 'Meteorologisk institutt',
    typeId: 3,
    type: 'Karttjenester',
    keyWords: ['infoMapAccessService', 'barentswatch']
}];

var countyList = [
    'Akershus',
    'Aust-Agder',
    'Buskerud',
    'Finnmark',
    'Hedmark',
    'Hordaland',
    'Møre og Romsdal',
    'Nordland',
    'Nord-Trøndelag',
    'Oppland',
    'Oslo',
    'Rogaland',
    'Sogn og Fjordane',
    'Sør-Trøndelag',
    'Telemark',
    'Troms',
    'Vest-Agder',
    'Vestfold',
    'Østfold'
];
