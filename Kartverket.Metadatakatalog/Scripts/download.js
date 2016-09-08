var objCount = 0;
var objCountLoaded = 0;

function httpToHttps(links) {
    $.each(links, function (key, val) {
        var protocol = location.protocol + "//";
        val['href'] = val['href'].replace(/^https?\:\/\//i, protocol);
    });
}

function getJsonObjects(data, segment, uuid) {
    if (segment == null) segment = '';
    var localStorageKey;
    var items = [];
    var parentObj = '';

    $.each(data, function (key, val) {
        if (segment != '' && segment != null) {
            localStorageKey = uuid + segment + '.' + key;
        } else {
            localStorageKey = uuid + key;
        }
        if (typeof val == 'object') {
            if (typeof key == 'string') {
                //if (key == '_links') {
                //    val = httpToHttps(val);
                //}
                parentObj = '.' + key;
            }
            $.each($(this), function (key, val) {
                items.push(val);
            });
        } else {
            localStorage[localStorageKey] = val;
        }

    });
    if (items.length > 0) {
        localStorageKey = uuid + segment + parentObj;
        localStorage[localStorageKey] = JSON.stringify(items);
    }
}

function getJsonUrl(data, rel) {
    var url = '';
    $.each(data, function (key, val) {
        if (val.rel == rel) {
            url = val.href;
        }
    });
    return url;
}

function fixUrl(urlen)
{
    urlJson = urlen.replace("%3F", "?");
    return urlJson;
}

function getJsonData(url, segments, uuid) {
    segmentUri = '';
    segmentString = '';
    $.each(segments, function (pos, segment) {
        segmentUri += '/' + segment;
        segmentString += '.' + segment;
    });

    var jsonUri = fixUrl(url);
    if (jsonUri != "")
    {
        $.ajax({
            url: jsonUri,
            dataType: 'json',
            async: false,
            error: function (jqXHR, textStatus, errorThrown) {
                var metadata = JSON.parse(localStorage.getItem(uuid + '.metadata'));
                showAlert(metadata.name + ' feilet. Vennligst fjern datasettet fra kurv. Feilmelding: ' + errorThrown + '<br />', 'danger');
                console.log('jqXHR:');
                console.log(jqXHR);
                console.log('textStatus:');
                console.log(textStatus);
                console.log('errorThrown:');
                console.log(errorThrown);
            },
            success: function (data) {
                if (data != null) {
                    getJsonObjects(data, segmentString, uuid);
                    if (data.supportsProjectionSelection) {
                        var rel = 'http://rel.geonorge.no/download/projection';
                        var href = getJsonUrl(data._links, rel);
                        getJsonData(href + '', ['codelists', 'projection'], uuid);
                    }
                    if (data.supportsFormatSelection) {
                        var rel = 'http://rel.geonorge.no/download/format';
                        var href = getJsonUrl(data._links, rel);
                        getJsonData(href, ['codelists', 'format'], uuid);
                    }
                    if (data.supportsAreaSelection) {
                        var rel = 'http://rel.geonorge.no/download/area';
                        var href = getJsonUrl(data._links, rel);
                        getJsonData(href, ['codelists', 'area'], uuid);
                    }
                }
                else
                {
                    var metadata = JSON.parse(localStorage.getItem(uuid + '.metadata'));
                    showAlert(metadata.name + " mangler data for å kunne lastes ned. Vennligst fjern datasettet fra kurv.<br />", 'danger');
                }
            }
        });
    }

    function showAlert(message, colorClass) {
        $('#feedback-alert').attr('class', 'alert alert-dismissible alert-' + colorClass);
        $('#feedback-alert .message').html( $('#feedback-alert .message').html() + message);
        $('#feedback-alert').show();
    }
}


$(document).ready(function () {
    var orderItems = localStorage["orderItems"];
    if (orderItems != null) {
        var storedOrderItems = (orderItems != null) ? JSON.parse(orderItems) : '';
        objCount = storedOrderItems.length;
        $.each(storedOrderItems, function (key, uuid) {
            var metadata = JSON.parse(localStorage[uuid + '.metadata']);
            var url = metadata.distributionUrl + uuid;
            getJsonData(url, ['capabilities'], uuid);
            objCountLoaded++;
            var percentLoaded = (objCountLoaded / objCount) * 100;
            $(".progress-bar").css('width', percentLoaded + '%');
            $(".progress-bar").attr('aria-valuenow', percentLoaded);
            $(".progress-bar").text(objCountLoaded + ' av ' + objCount + ' datasett er lastet');
        });
    }

    var mapLoaded = false;

    // Legge til uuid i _NorgesKart
    $('#orderlist').on('click', 'button.selectPolygon-button', (function (e) {
        var uuid = $(this).attr('uuid');
        var orderItemSelectOmraader = $('select[name=' + uuid + '-areas]');
        var supportspolygonfixedselection = orderItemSelectOmraader.attr('supportspolygonfixedselection');
        var areatype = orderItemSelectOmraader.attr('areatype');
        var mapselectionlayer = orderItemSelectOmraader.attr('mapselectionlayer');
        //if (!mapLoaded)
        loadMap(uuid, supportspolygonfixedselection, areatype, mapselectionlayer);
        //mapLoaded = true; //Need to load each time to set coverageMap.
        $('#norgeskartmodal #setcoordinates').attr('uuid', uuid);
    }));
});


// Populering av Områdeliste
function populateAreaList(uuid, supportsAreaSelection, supportsPolygonSelection) {
    if (supportsAreaSelection) {

        var orderItemOmraader = (JSON.parse(localStorage.getItem(uuid + '.codelists.area')));
        var orderItemSelectOmraader = $('#orderuuid' + uuid + ' .selectOmraader');
        orderItemSelectOmraader.attr('name', uuid + '-areas');
        orderItemSelectOmraader.attr('id', uuid + '-areas');
        orderItemSelectOmraader.attr('onchange', 'HandleChange(\'' + uuid + '\')');

        var omraadeTypes = [];
        $.each(orderItemOmraader, function (key, val) {
            if (omraadeTypes.indexOf(val.type) == -1) {
                omraadeTypes.push(val.type);
            }
        });

        for (omraade in omraadeTypes) {
            if (omraadeTypes[omraade] != "landsdekkende"){
                orderItemSelectOmraader.append($('<optgroup label="' + omraadeTypes[omraade] + '" class="selectOmraader' + omraadeTypes[omraade] + '"  />'));
            }

            var orderItemSelectOmraaderType = $('#orderuuid' + uuid + ' .selectOmraader .selectOmraader' + omraadeTypes[omraade]);
         
            $.each(orderItemOmraader, function (key, val) {
                if (val.type == 'landsdekkende') {
                    if (orderItemSelectOmraader.find('option[value="landsdekkende_0000"]').length == 0) {
                        orderItemSelectOmraader.prepend($("<option />").val(val.type + '_' + val.code).text('Hele landet'));
                    }
                }
                else if (val.type == omraadeTypes[omraade]) {
                    orderItemSelectOmraaderType.append($("<option />").val(val.type + '_' + val.code).text(val.name));
                }
            });

            if (omraadeTypes[omraade] == "kartblad" || omraadeTypes[omraade] == "celle") {
                supportsPolygonSelection = true;
                orderItemSelectOmraader.attr('supportspolygonfixedselection', 'true');
                orderItemSelectOmraader.attr('areatype', omraadeTypes[omraade]);

                var mapSelectionLayer = localStorage.getItem(uuid + '.capabilities.mapSelectionLayer');
                orderItemSelectOmraader.attr('mapselectionlayer', mapSelectionLayer);

            }
        }

        orderItemSelectOmraader.change(function () {
            populateProjectionAndFormatList(uuid, orderItemOmraader);
        });

    } else {
        var formElement = $('#orderuuid' + uuid + ' .selectOmraader');
        formElement.attr('disabled', 'disabled');
    }
    if (!supportsPolygonSelection) {
        var formElement = $('#orderuuid' + uuid + ' .btn');
        formElement.addClass('disabled');
        formElement.parent(".selectPolygon").removeAttr('data-toggle');
        formElement.attr('disabled', true);
    }
}

// Populering av projeksjon- og format-liste
function populateProjectionAndFormatList(uuid, orderItemOmraader) {

    if(orderItemOmraader != null)
    {
        var coordinates = localStorage.getItem([uuid + '.selected.coordinates']);
        if (coordinates == null || coordinates == '') {

            var selectedAreas = $('[name=\'' + uuid + "-areas']").val();

            var orderItemSelectProjeksjoner = $('#orderuuid' + uuid + ' .selectProjeksjoner');
            orderItemSelectProjeksjoner.attr('name', uuid + '-projection');
            orderItemSelectProjeksjoner.empty();
            orderItemSelectProjeksjoner.trigger("chosen:updated");

            var orderItemSelectFormater = $('#orderuuid' + uuid + ' .selectFormater');
            orderItemSelectFormater.attr('name', uuid + '-formats');
            orderItemSelectFormater.empty();

            var supportsProjectionSelection = (localStorage.getItem(uuid + '.capabilities.supportsProjectionSelection') === "true");
            if (supportsProjectionSelection)
                orderItemSelectProjeksjoner.attr("disabled", false);
            else
                orderItemSelectProjeksjoner.attr("disabled", true);

            orderItemSelectProjeksjoner.trigger("chosen:updated");

            var supportsFormatSelection = (localStorage.getItem(uuid + '.capabilities.supportsFormatSelection') === "true");

            if (supportsFormatSelection)
                orderItemSelectFormater.attr("disabled", false);
            else
                orderItemSelectFormater.attr("disabled", true);

            orderItemSelectFormater.trigger("chosen:updated");


            $.each(orderItemOmraader, function (key, val) {
                if ($.inArray(val.type + "_" + val.code, selectedAreas) > -1) {

                    $.each(val.projections, function (key, val) {
                        if (orderItemSelectProjeksjoner.find('option[value="' + val.code + '"]').length <= 0) {
                            orderItemSelectProjeksjoner.append($("<option />").val(val.code).text(val.name));
                            orderItemSelectProjeksjoner.trigger("chosen:updated");
                        }

                    });
                    $.each(val.formats, function (key, val) {
                        if (orderItemSelectFormater.find('option[value="' + val.name + '"]').length <= 0) {
                            orderItemSelectFormater.append($("<option />").val(val.name).text(val.name));
                            orderItemSelectFormater.trigger("chosen:updated");
                        }
                    });
                }
            });
        }
    }

}


// Populering av Projeksjonliste
function populateProjectionList(uuid, supportsProjectionSelection) {
    if (supportsProjectionSelection) {
        var orderItemProjeksjoner = (JSON.parse(localStorage.getItem(uuid + '.codelists.projection')));
        var orderItemSelectProjeksjoner = $('#orderuuid' + uuid + ' .selectProjeksjoner');
        orderItemSelectProjeksjoner.attr('name', uuid + '-projection');
        $.each(orderItemProjeksjoner, function (key, val) {
            orderItemSelectProjeksjoner.append($("<option />").val(val.code).text(val.name));
        });
    } else {
        var formElement = $('#orderuuid' + uuid + ' .selectProjeksjoner');
        formElement.attr("disabled", true);
    }
}

// Populering av Filformatliste
function populateFormatList(uuid, supportsFormatSelection) {
    if (supportsFormatSelection) {
        var orderItemFormater = (JSON.parse(localStorage.getItem(uuid + '.codelists.format')));
        var orderItemSelectFormater = $('#orderuuid' + uuid + ' .selectFormater');
        orderItemSelectFormater.attr('name', uuid + '-formats');
        $.each(orderItemFormater, function (key, val) {
            orderItemSelectFormater.append($("<option />").val(val.name).text(val.name));
        });
    } else {
        var formElement = $('#orderuuid' + uuid + ' .selectFormater');
        formElement.attr("disabled", true);
    }
}


// Hent ut order-url
function getOrderUrl(uuid) {
    var orderItemLinks = JSON.parse(localStorage.getItem(uuid + '.capabilities._links'));
    var orderItemInputOrderUrl = $('#orderuuid' + uuid + ' .order-url');
    orderItemInputOrderUrl.attr('name', uuid + '-orderUrl');

    var orderRel = "http://rel.geonorge.no/download/order";
    if (orderItemLinks != null)
    {
        $.each(orderItemLinks, function (key, link) {
            if (link['rel'] == orderRel) {
                orderItemInputOrderUrl.val(link['href']);
            }
        });
    }
}


// Populering av Koordinatfelt
function getSelectedCoordinates(uuid, selectClass, name) {
    var coordinates = localStorage.getItem([uuid + '.selected.' + name]);
    var orderItemInputCoordinates = $('#orderuuid' + uuid + ' .coordinates');
    var selectField = $('#orderuuid' + uuid + ' select.selectOmraader');
    if (coordinates != null && coordinates != '') {
        orderItemInputCoordinates.attr('name', uuid + '-' + name);
        orderItemInputCoordinates.val(coordinates);
        if (coordinates == "undefined"){
            showAlert('Feil: Koordinater kunne ikke bli valgt fra kartet', 'danger');
            removeCoordinates(uuid)
        }
        else
        {
            selectField.children().remove("option[class='from-map']");
            selectField.append('<option class="from-map" selected="selected" value="0">Valgt fra kart</option>');
            selectField.trigger("chosen:updated");
        }
    }
}

// Hent ut selected verdier fra localStorage
function getSelectedValues(orderItems, selectClass, name) {
    if (name == 'coordinates') {
        getSelectedCoordinates(orderItems, selectClass, name);

    } else {
        $.each(orderItems, function (key, uuid) {
            var select = $('#orderuuid' + uuid).find('.' + selectClass);
            var selectedValues = JSON.parse(localStorage.getItem([uuid + '.selected.' + name]));
            $.each(select, function (key, val) {
                var option = $(this).find('option');

                option.each(function (i, selected) {
                    if ($.inArray($(this).val(), selectedValues) > -1) {
                        $(this).attr('selected', 'selected');
                    }
                });
            });
        });
    }
}


function generateView(template, orderItems) {
    $.each(orderItems, function (key, uuid) {
        $("#orderlist").append('<div id="orderuuid' + uuid + '" class="order-item">' + template + '</div>');
        $("#orderuuid" + uuid + " .remove-item").attr('uuid', uuid);
        $("#orderuuid" + uuid + " .selectPolygon button").attr('uuid', uuid);

        var supportsProjectionSelection = (localStorage.getItem(uuid + '.capabilities.supportsProjectionSelection') === "true");
        var supportsFormatSelection = (localStorage.getItem(uuid + '.capabilities.supportsFormatSelection') === "true");
        var supportsPolygonSelection = (localStorage.getItem(uuid + '.capabilities.supportsPolygonSelection') === "true");
        var supportsAreaSelection = (localStorage.getItem(uuid + '.capabilities.supportsAreaSelection') === "true");
        var metadata = JSON.parse(localStorage.getItem(uuid + '.metadata'));
        $("#orderuuid" + uuid + " .order-title").html(metadata.name);
        $("#orderuuid" + uuid + " .order-title").attr('href', metadata.url);
        $("#orderuuid" + uuid + " .order-img").attr('src', metadata.organizationLogoUrl);
        $("#orderuuid" + uuid + " .order-img").attr('alt', metadata.organizationName);

        populateAreaList(uuid, supportsAreaSelection, supportsPolygonSelection);
        populateProjectionList(uuid, supportsProjectionSelection);
        populateFormatList(uuid, supportsFormatSelection);
        
        getOrderUrl(uuid);

        var orderItems = JSON.parse(localStorage["orderItems"]);
        getSelectedValues(orderItems, 'selectProjeksjoner', 'projections');
        getSelectedValues(orderItems, 'selectFormater', 'formats');
        getSelectedValues(orderItems, 'selectOmraader', 'areas');
        var orderItemOmraader = (JSON.parse(localStorage.getItem(uuid + '.codelists.area')));
        populateProjectionAndFormatList(uuid, orderItemOmraader);
        getSelectedValues(uuid, 'coordinates', 'coordinates');
    });
}

function showAlert(message, colorClass) {
    $('#feedback-alert').attr('class', 'alert alert-dismissible alert-' + colorClass);
    $('#feedback-alert .message').text(message);
    $('#feedback-alert').show();
}

$(window).load(function () {
    $(".progress").fadeOut("slow");


    // Fjerning av datasett fra handlekurv
    $('#orderlist').on('click', '.remove-item', (function (e) {
        var uuid = $(this).attr('uuid');
        var uuidLength = uuid.length;
        var orderItems = JSON.parse(localStorage["orderItems"]);
        orderItems = $.grep(orderItems, function (value) {
            return value != uuid;
        });
        localStorage["orderItems"] = JSON.stringify(orderItems);
        Object.keys(localStorage)
              .forEach(function (key) {
                  if (key.substring(0, uuidLength) == uuid) {
                      localStorage.removeItem(key);
                  }
              });
        $(this).closest('.order-item').remove();
        updateShoppingCart();
        updateShoppingCartCookie();
    }));

    // Fjerning av alle datasett fra handlekurv
    $('#remove-all-items').click(function () {
        $('#remove-all-items-modal').modal('show')
    });

    $('#remove-all-items-submit').click(function () {

        Object.keys(localStorage)
              .forEach(function (key) {
                  if (key != 'visningstype') {
                      localStorage.removeItem(key);
                  }
              });
        $('#remove-all-items-modal').modal('hide')
        $('.order-item').remove();
        updateShoppingCart();
        updateShoppingCartCookie();
    });


    // Lagre selected verdier i localStorage
    function setSelectedValues(orderItems, selectClass, name) {
        $.each(orderItems, function (key, val) {
            var select = $('#orderuuid' + val).find('.' + selectClass);
            var uuid = val;
            $.each(select, function (key, val) {
                var selectedValues = [];
                var option = $(this).find('option:selected');
                var selectedCount = $(this).find('option:selected').length;
                if (selectedCount > 0) {
                    var formgroup = $(this).parent('.form-group');
                    formgroup.find('.chosen-choices').removeClass('has-error');
                }
                option.each(function (i, selected) {
                    selectedValues[i] = $(selected).val();
                });
                localStorage[uuid + '.selected.' + name] = JSON.stringify(selectedValues);
            });
        });
    }

    $('#orderlist').on('change', 'select', (function (e) {
        var orderItems = JSON.parse(localStorage["orderItems"]);
        setSelectedValues(orderItems, 'selectProjeksjoner', 'projections');
        setSelectedValues(orderItems, 'selectFormater', 'formats');
        setSelectedValues(orderItems, 'selectOmraader', 'areas');
    }));

    $('.body-content').on('click', '#download-btn', (function (e) {
        var formgroup = $('.body-content').find('.list-group-item .form-group');
        var hasErrors = false;
        $.each(formgroup, function () {
            var selectedCount = $(this).find('option:selected').length;
            if (selectedCount < 1) {
                $(this).find('.chosen-choices').addClass('has-error');
                hasErrors = true;
            } else {
                $(this).find('.chosen-choices').removeClass('has-error');
            }
        });
        if (hasErrors) {
            showAlert('Ett eller flere felt er ikke korrekt utfylt', 'danger');
        } else if (!isValid()) {
            $('#emailField').addClass('has-error')
            showAlert('E-post feltet må fylles ut om man har valgt koordinater fra kart', 'danger');
            e.preventDefault();
        }

        // ga track
        if (!hasErrors && isValid())
        {
            var orderItems = localStorage["orderItems"];
            if (orderItems != null) {
                var storedOrderItems = (orderItems != null) ? JSON.parse(orderItems) : '';
                objCount = storedOrderItems.length;
                $.each(storedOrderItems, function (key, uuid) {
                    var metadata = JSON.parse(localStorage[uuid + '.metadata']);
                    ga('send', 'event', 'Nedlasting', 'Hent nedlastingslenke', metadata.name);
                });
            }
        }

    }));
});

function removeCoordinates(uuid) {
    selectField = $('#orderuuid' + uuid + ' select.selectOmraader');
    selectField.children().remove("option[class='from-map']");
    selectField.trigger("chosen:updated");
    localStorage.setItem(uuid + '.selected.coordinates', '');
    var orderItemInputCoordinates = $('#orderuuid' + uuid + ' .coordinates');
    orderItemInputCoordinates.val('');
}


function HandleChange(uuid) {
    var selectedValues = $("#" + uuid + "-areas").val();
    if ($.inArray("0", selectedValues) == -1)
    {
        removeCoordinates(uuid);
    }
}


function containsCoordinates() {
    var coordinateFields = $('.coordinates');
    var containsCoordinates = false
    $.each(coordinateFields, function () {
        if ($(this).val() != '') {
            containsCoordinates = true;
        }
    });
    return containsCoordinates;
}

function containsEmail() {
    var containsEmail = ($('#emailField input').val() != '') ? true : false;
    return containsEmail;
}

function isValid() {
    var validWithCoordinates = (containsCoordinates() && containsEmail()) ? true : false;
    var validWithoutCoordinates = (!containsCoordinates()) ? true : false;
    return (validWithCoordinates || validWithoutCoordinates) ? true : false;
}