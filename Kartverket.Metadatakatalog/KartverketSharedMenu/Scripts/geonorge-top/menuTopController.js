

(function () {
    var app = angular.module("geonorge");
    var baseurl = searchOption.geonorgeUrl;
    app.controller('menuTopController', [
      '$scope', '$http',
      function ($scope, $http) {
         
          function handleSuccess(respons) {

              localStorage.setItem('menuItems', JSON.stringify(respons));
              var date = new Date();
              var minutes = 3;
              date.setTime(date.getTime() + (minutes * 60 * 1000));
              $.cookie('expire', "menu", { expires: date });

              $scope.menuItems = respons.data;
          }

          function handleError() {
              $scope.getMenuError = true;
          }

          $scope.getMenuData = function getMenuData() {

              if (!$.cookie('expire') || !localStorage.getItem('menuItems')) {

                  var menuService = baseurl + '/api/menu';
                  var request = $http({
                      method: 'GET',
                      url: menuService,
                      headers: {
                          'Content-Type': 'application/json; charset=utf-8'

                      },
                      data: {}
                  });

                  console.log("Menu loaded from server");
                  
                  return request.then(handleSuccess, handleError);


              }
              else
              {
                  console.log("Menu loaded locally");
                  response = JSON.parse(localStorage.getItem('menuItems'));
                  $scope.menuItems = response.data;
              }
          }
         
      }]);
}());
