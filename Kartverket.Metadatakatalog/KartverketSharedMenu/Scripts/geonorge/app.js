angular.module('geonorge', ['ui.bootstrap']);

angular.module('geonorge').config(function ($sceDelegateProvider) {
    $sceDelegateProvider.resourceUrlWhitelist(['**']);
});