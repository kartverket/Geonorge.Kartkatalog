var baseurl = 'http://' + window.location.host;

(function () {
    var app = angular.module("geonorge");
    
    app.controller('searchController', [
      '$rootScope', '$scope', '$location', '$window', '$timeout', 'aggregatedService', '$sce',
      function ($rootScope, $scope, $location, $window, $timeout, aggregatedService, $sce) {

          $rootScope.trustHtml = function (html) {
              return $sce.trustAsHtml(html);
          };

          $scope.dropdownOpen = false;
          $scope.extendedSearchOpen = false;
          $scope.showFakeResults = false;
          $scope.searchString = "";
          var queryString = getQueryStringParams();
          var selectedSearchIndex = isNaN(queryString['section']) ? 0 : parseInt(queryString['section']);
          $rootScope.selectedSearch =  dropdownOptions[selectedSearchIndex];
          $scope.dropdownOptions = dropdownOptions;
          $rootScope.searchQuery = '';
          $scope.autoCompleteResult = [];
          $scope.autoCompletePartial = baseurl + '/Scripts/Geonorge/partials/_autoCompleteRow.html';
          $scope.focused = false;
          $scope.autocompleteActive = false;
          $scope.ajaxCallActive = false;
          $scope.allowBlur = true;
          $scope.viewport = {
              width: window.innerWidth,
              height: window.innerHeight
          };
          $scope.breakpoints = {
              xSmall: 480,
              small: 768,
              medium: 992,
              large: 1200
          };

          var select = function (e, search) {
              e.preventDefault();
              e.stopPropagation();
              $rootScope.selectedSearch = search;
              $scope.dropdownOpen = false;
              var txt = document.getElementById('txtSearch');
              txt.focus();
          };

          $scope.select = select;

          
          var buttonDropdownKeyDown = function (e) {
              var dropdown;
              switch (e.keyCode) {
                  //Enter on button is handeled fine by default
                  case 38: //Arrow up
                      e.target.blur();
                      dropdown = angular.element(e.target).next();
                      dropdown.children()[dropdownOptions.length - 1].focus();
                      break;
                  case 40: //Arrow down
                      e.target.blur();
                      dropdown = angular.element(e.target).next();
                      dropdown.children()[0].focus();
                      break;
                  default:
                      return;
              }
              e.preventDefault();
              e.stopPropagation();
          };

          var dropdownKeyDown = function (e, id) {
              var toFocus;
              switch (e.keyCode) {
                  case 13: //Enter
                      select(e, dropdownOptions[id]);
                      return;
                  case 38: //arrow-up
                      var dropdown = angular.element(document.getElementById("search-dropdown"));
                      if (id === 0) { //Wrap-around
                          toFocus = dropdown.children()[dropdownOptions.length - 1];
                      } else {
                          toFocus = dropdown.children()[id - 1];
                      }
                      break;
                  case 40: //arrow down
                      if (id >= dropdownOptions.length - 1) {  //Wrap-around
                          toFocus = angular.element(document.getElementById("search-dropdown")).children()[0];
                      } else {
                          toFocus = angular.element(e.target).next()[0];
                      }
                      break;
                  default:
                      return; //Dont stop propagation/default
              }
              e.target.blur();
              toFocus.focus();
              e.preventDefault();
              e.stopPropagation();
          };

          $scope.dropdownKeyDown = dropdownKeyDown;
          $scope.buttonDropdownKeyDown = buttonDropdownKeyDown;

          $scope.onSearch = function (ev) {
              if (ev) ev.preventDefault();
              if ($rootScope.searchQuery.length < 3) return;
              if (!$scope.selectedSearch.localUrl) {
                  fallbackRouting();
                  return;
              }
              //sessionStorage.clear();
              //The service tries to trigger the connected search method - if not, the fallback method is used
              var src = aggregatedService.triggerSearch($rootScope.searchQuery);
              //Specify a fallback in case aggregatedService doesn't have a method for search implemented
              src.then(fallbackRouting);
          };

          function fallbackRouting() {
              var search = $scope.selectedSearch;
              var param = '';
              if ($rootScope.searchQuery != '') {
                  param = search.queryParameter;
                  param += $rootScope.searchQuery;
              }

              var relativeUrl = search.url + param;
              $window.location.href = relativeUrl;
          }

          $scope.preventDefault = function (ev) {

              switch (ev.keyCode) {
                  case 13:
                      ev.preventDefault();
                      break;
                  case 16:
                      shiftKey = true;
                      break;
                  case 9:
                      if ($scope.autoCompleteResult.length > 0) {// && categoryCount <= $scope.autoCompleteResult.length && resultCount < $scope.autoCompleteResult[$scope.autoCompleteResult.length -1].list.length) {
                          ev.preventDefault();
                      }
                      break;
                  case 38:
                  case 40:
                      ev.preventDefault();
                      break;
              }
          };

          var timer = null;
          $scope.autocomplete = function (ev) {
              if ($scope.viewport.width <= $scope.breakpoints.small) return;

              if ($scope.focused == false) return;

              if ($rootScope.searchQuery.length < 3) {
                  $scope.autoCompleteResult = [];
                  $scope.autocompleteActive = false;
                  $scope.ajaxCallActive = false;
                  categoryCount = null;
                  return;
              }

              switch (ev.keyCode) {
                  //enter                                                                                                                                                            
                  case 13:
                      if ($scope.selectedSearch.autoComplete) {
                          if (categoryCount == null) {
                              $scope.resetAutocomplete();
                              $scope.allowBlur = true;
                              $scope.onSearch(ev);

                          } else {
                              $scope.allowBlur = false;
                              window.location = $scope.autoCompleteResult[categoryCount - 1].list[resultCount - 1].url;
                          }
                      } else $scope.onSearch(ev);
                      break;
                  case 16:
                      shiftKey = false;
                      break;
                      //left                                                                                                                                                            
                  case 37:
                      break;
                      //up                                                                                                                                                            
                  case 38:
                      autoCompleteMoveUp();
                      return false;
                      //right
                  case 39:
                      break;
                      //Tab                                                                                                                                                            
                  case 9:
                      if (!shiftKey) {
                          autoCompleteMoveDown();
                      } else {
                          autoCompleteMoveUp();
                      }
                      break;
                      //down                                                                                                                                                            
                  case 40:
                      autoCompleteMoveDown();

                      return false;
                  default:
                      if (!$scope.selectedSearch.autoComplete) return;
                      categoryCount = null;
                      if (timer) {
                          $timeout.cancel(timer);
                          timer = null;
                          console.log('cancel timeout');
                      }

                      timer = $timeout(function() {
                          $scope.autocompleteActive = true;
                          console.log('calling WS');
                          if ($rootScope.searchQuery.length > 0) {
                              $scope.ajaxCallActive = true;
                              // TEST! aggregatedService.performSearch($rootScope.searchQuery, [], 5, $scope.selectedSearch.section).then(displayAutoCompleteData, errorHandler);
                              aggregatedService.performSearch($rootScope.searchQuery, [], 5, 0).then(displayAutoCompleteData, errorHandler);
                          }
                      }, 300);
                      return;
              }
          };

          function displayAutoCompleteData(response) {
              $scope.ajaxCallActive = false;
              $scope.autoCompleteResult = [];
              if (response.d) {
                  var list = [];

                  if (response.d.NumberOfHitsTotal == 0) {
                      $scope.autoCompleteResult = [];
                      return;
                  }

                  //list.push(response.d.Metadata);
                  //list.push(response.d.Geonorge);
                  list = response.d.Results;

                  for (var x = 0; x < list.length; x++) {
                      var item = {};
                      var curr = list[x];
                      if (curr.Results.length == 0) continue;
                      item.type = curr.Section;
                      item.title = curr.SectionName;
                      item.list = [];
                      for (var y = 0; y < curr.Results.length; y++) {
                          var currResult = curr.Results[y];

                          item.list.push({
                              externalId: curr.SectionName + '_' + curr.Section + '_' + y,
                              id: y,
                              typeId: curr.Section,
                              title: currResult.Title,
                              url: currResult.Url
                          });
                      }
                      $scope.autoCompleteResult.push(item);
                      console.log(item);
                  }


                  //for (var i = 0; i < response.d.Geonorge.Results.length; i++) {
                  //    var curr = response.d.Geonorge.Results[i];
                  //    console.log(curr.Title);
                  //}
              };
          }

          function errorHandler(errorMessage) {
              $scope.resetAutocomplete();
              console.log(errorMessage);
          }

          var categoryCount = null;
          var resultCount = null;
          var shiftKey = false;
          function autoCompleteMoveUp() {

              if (resultCount > 0 && categoryCount == 1) {
                  resultCount--;
                  if (resultCount == 0) categoryCount = null;
              }

              if (resultCount == 1 && categoryCount > 1) {
                  categoryCount--;
                  resultCount = $scope.autoCompleteResult[categoryCount - 1].list.length;
              }



              if (categoryCount > 1 & resultCount > 1) {
                  resultCount--;
              }

              setHighlightedRow();
          }

          function autoCompleteMoveDown() {
              if (categoryCount == null) {
                  categoryCount = 1;
                  resultCount = 1;
              } else {
                  if (categoryCount == $scope.autoCompleteResult.length) {
                      if ($scope.autoCompleteResult[categoryCount - 1].list.length > resultCount) {
                          resultCount++;
                      }
                  }
                  if (categoryCount < $scope.autoCompleteResult.length) {
                      if ($scope.autoCompleteResult[categoryCount - 1].list.length > resultCount) {
                          resultCount++;
                      }
                      else {
                          categoryCount++;
                          resultCount = 1;
                      }
                  }
              }
              setHighlightedRow();

          }

          function setHighlightedRow() {
              for (var x = 0; x < $scope.autoCompleteResult.length; x++) {
                  var curr = $scope.autoCompleteResult[x];
                  if (x == categoryCount - 1) {
                      for (var y = 0; y < curr.list.length; y++) {
                          var innerItem = curr.list[y];
                          if (y == resultCount - 1) {
                              innerItem.highlight = true;
                          } else {
                              innerItem.highlight = false;
                          }
                      }

                  } else {
                      for (var z = 0; z < curr.list.length; z++) {

                          curr.list[z].highlight = false;
                      }
                  }
              }
              console.log('categoryCount ' + categoryCount);
              console.log('resultCount ' + resultCount);
          }

          $scope.mouseOver = function (val, category, index) {
              console.log(category);
              console.log(index);
              $scope.allowBlur = val;
              resultCount = index + 1;
              categoryCount = category + 1;
              setHighlightedRow();
          };
          $scope.mouseOut = function (val) {
              $scope.allowBlur = val;
          };

          $scope.resetAutocomplete = function () {
              $scope.focused = false;
              $scope.autocompleteActive = false;
              $scope.ajaxCallActive = false;
              $scope.autoCompleteResult = [];
          };

          $scope.setFocus = function (ev) {
              $scope.focused = true;
              console.log($scope.focused);
              angular.element(ev.target).on('blur', function () {
                  $timeout(function () {
                      if ($scope.allowBlur) {
                          $scope.resetAutocomplete();
                          console.log($scope.focused);
                          angular.element(ev.target).on('blur', null);
                      }
                  }, true);
              });
          };

          //angular.element(document).ready(function () {
          //    aggregatedService.triggerSearch($rootScope.searchQuery);
          //});
      }]);
}());
