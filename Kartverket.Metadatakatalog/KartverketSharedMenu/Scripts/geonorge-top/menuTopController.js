
(function () {
    var app = angular.module("geonorge");

    app.controller('menuTopController', [
      '$scope', '$http',
      function ($scope, $http) {
         
          function handleSuccess(respons) {
              $scope.menuItems = respons.data;
          }

          function handleError() {
              $scope.getMenuError = true;
          }

          $scope.getMenuData = function getMenuData() {
              var menuService = baseurl + '/api/menu';
              var request = $http({
                  method: 'GET',
                  url: menuService,
                  headers: {
                      'Content-Type': 'application/json; charset=utf-8',
                      'accept': '*/*'
                  },
                  data: {}
              });

              return request.then(handleSuccess, handleError);
          }
         
      }]);
}());
