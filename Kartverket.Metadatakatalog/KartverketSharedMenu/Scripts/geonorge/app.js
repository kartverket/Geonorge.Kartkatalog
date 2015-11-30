angular.module('geonorge', ['ui.bootstrap']);

var app = angular.module('geonorge', ['ui.bootstrap']);
app.controller('MainCtrl', function ($scope, $sce) {
    $scope.trustSrc = function (src) {
        return $sce.trustAsResourceUrl(src);
    }

    $scope.url = { src: "http://kartkatalog.dev.geonorge.no/KartverketSharedMenu/Scripts/geonorge/partials/_autoCompleteRow.html" };
});