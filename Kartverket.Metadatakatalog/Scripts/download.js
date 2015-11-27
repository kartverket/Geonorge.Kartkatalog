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
                if (key == '_links') {
                    val = httpToHttps(val);
                }
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


function getJsonData(url, segments, uuid) {
    segmentUri = '';
    segmentString = '';
    $.each(segments, function (pos, segment) {
        segmentUri += '/' + segment;
        segmentString += '.' + segment;
    });
    var jsonUri = url + '?json=true';
    $.ajax({
        url: jsonUri,
        dataType: 'json',
        async: false,
        success: function (data) {
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
    });
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


    // Legge til uuid i _NorgesKart
    $('#orderlist').on('click', 'button.selectPolygon-button', (function (e) {
        var uuid = $(this).attr('uuid');
        $('#norgeskartmodal #setcoordinates').attr('uuid', uuid);
    }));
});


// Populering av Områdeliste
function populateAreaList(uuid, supportsAreaSelection, supportsPolygonSelection) {
    if (supportsAreaSelection) {

        var orderItemOmraader = (JSON.parse(localStorage.getItem(uuid + '.codelists.area')));
        var orderItemSelectOmraader = $('#orderuuid' + uuid + ' .selectOmraader');
        orderItemSelectOmraader.attr('name', uuid + '-areas');
        orderItemSelectOmraader.change(function () {
            populateProjectionAndFormatList(uuid, orderItemOmraader);
        });
        var orderItemSelectOmraaderFylker = $('#orderuuid' + uuid + ' .selectOmraader .selectOmraaderFylker');
        var orderItemSelectOmraaderKommuner = $('#orderuuid' + uuid + ' .selectOmraader .selectOmraaderKommuner');
        $.each(orderItemOmraader, function (key, val) {
            if (val.type == 'fylke') {
                orderItemSelectOmraaderFylker.append($(
                    "<option />").val(val.type + '_' + val.code).text(val.name));
            }
            else if (val.type == 'kommune') {
                orderItemSelectOmraaderKommuner.append($("<option />").val(val.type + '_' + val.code).text(val.name));
            }
            else if (val.type == 'landsdekkende') {
                orderItemSelectOmraader.prepend($("<option />").val(val.type + '_' + val.code).text('Hele landet'));
            }
            else {
                orderItemSelectOmraader.append($("<option />").val(val.code).text(val.name));
            }
        });
    } else {
        var formElement = $('#orderuuid' + uuid + ' .selectOmraader');
        formElement.attr('disabled', 'disabled');
    }
    if (supportsPolygonSelection == 'false') {
        var formElement = $('#orderuuid' + uuid + ' .btn');
        formElement.addClass('disabled');
        formElement.attr('disabled', true);
    }
}

// Populering av projeksjon- og format-liste
function populateProjectionAndFormatList(uuid, orderItemOmraader) {

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

        orderItemSelectProjeksjoner.attr("disabled", true);

        orderItemSelectProjeksjoner.trigger("chosen:updated");

        orderItemSelectFormater.attr("disabled", true);
        orderItemSelectFormater.trigger("chosen:updated");


        $.each(orderItemOmraader, function (key, val) {
            if ($.inArray(val.type + "_" + val.code, selectedAreas) > -1) {

                orderItemSelectProjeksjoner.attr("disabled", false);
                orderItemSelectFormater.attr("disabled", false);

                $.each(val.projections, function (key, val) {
                    if (orderItemSelectProjeksjoner.find('option[value="' + val.code + '"]').length <= 0) {
                        orderItemSelectProjeksjoner.append($("<option selected />").val(val.code).text(val.name));
                        orderItemSelectProjeksjoner.trigger("chosen:updated");
                    }

                });
                $.each(val.formats, function (key, val) {
                    if (orderItemSelectFormater.find('option[value="' + val.name + '"]').length <= 0) {
                        orderItemSelectFormater.append($("<option selected />").val(val.name).text(val.name));
                        orderItemSelectFormater.trigger("chosen:updated");
                    }
                });
            }
        });
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
        var formElement = $('#orderuuid' + uuid + ' .selectProjeksjoner').closest('.form-group');
        formElement.addClass('disabled');
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
        var formElement = $('#orderuuid' + uuid + ' .selectFormater').closest('.form-group');
        formElement.addClass('disabled');
    }
}


// Hent ut order-url
function getOrderUrl(uuid) {
    var orderItemLinks = JSON.parse(localStorage.getItem(uuid + '.capabilities._links'));
    var orderItemInputOrderUrl = $('#orderuuid' + uuid + ' .order-url');
    orderItemInputOrderUrl.attr('name', uuid + '-orderUrl');

    var orderRel = "http://rel.geonorge.no/download/order";

    $.each(orderItemLinks, function (key, link) {
        if (link['rel'] == orderRel) {
            orderItemInputOrderUrl.val(link['href']);
        }
    });
}


// Populering av Koordinatfelt
function getSelectedCoordinates(uuid, selectClass, name) {
    var coordinates = localStorage.getItem([uuid + '.selected.' + name]);
    var orderItemInputCoordinates = $('#orderuuid' + uuid + ' .coordinates');
    var selectField = $('#orderuuid' + uuid + ' select.selectOmraader');
    if (coordinates != null && coordinates != '') {
        orderItemInputCoordinates.attr('name', uuid + '-' + name);
        orderItemInputCoordinates.val(coordinates);
        selectField.html('<option class="from-map" selected="selected" value="0">Valgt fra kart</option>');
        selectField.trigger("chosen:updated");
        var selectGroup = $('#orderuuid' + uuid + ' .input-group-omraade');
        selectGroup.find('.search-choice-close').attr('onclick', 'removeCoordinates(\'' + uuid + '\')');
        $(window).load(function () {
            selectGroup.find('.search-choice-close').attr('onclick', 'removeCoordinates(\'' + uuid + '\')');
        });

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

        var supportsProjectionSelection = localStorage.getItem(uuid + '.capabilities.supportsProjectionSelection');
        var supportsFormatSelection = localStorage.getItem(uuid + '.capabilities.supportsFormatSelection');
        var supportsPolygonSelection = localStorage.getItem(uuid + '.capabilities.supportsPolygonSelection');
        var supportsAreaSelection = localStorage.getItem(uuid + '.capabilities.supportsAreaSelection');
        var metadata = JSON.parse(localStorage.getItem(uuid + '.metadata'));

        $("#orderuuid" + uuid + " .order-title").text(metadata.name);
        $("#orderuuid" + uuid + " .order-title").attr('href', metadata.url);
        $("#orderuuid" + uuid + " .order-img").attr('src', metadata.organizationLogoUrl);

        populateProjectionList(uuid, supportsProjectionSelection);
        populateFormatList(uuid, supportsFormatSelection);
        populateAreaList(uuid, supportsAreaSelection, supportsPolygonSelection);
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


$(window).load(function () {
    $(".progress").fadeOut("slow");
    $(".chosen-select").chosen();
    $('[data-toggle="tooltip"]').tooltip();

    function showAlert(message, colorClass) {
        $('#feedback-alert').attr('class', 'alert alert-dismissible alert-' + colorClass);
        $('#feedback-alert .message').text(message);
        $('#feedback-alert').show();
    }





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
    }));
});

function removeCoordinates(uuid) {
    selectField = $('#orderuuid' + uuid + ' select.selectOmraader');
    var supportsPolygonSelection = localStorage.getItem(uuid + '.capabilities.supportsPolygonSelection');
    var supportsAreaSelection = localStorage.getItem(uuid + '.capabilities.supportsAreaSelection');
    selectField.html('<optgroup label="Fylker" class="selectOmraaderFylker"></optgroup><optgroup label="Kommuner" class="selectOmraaderKommuner"></optgroup>');
    populateAreaList(uuid, supportsAreaSelection, supportsPolygonSelection);
    selectField.trigger("chosen:updated");
    localStorage.setItem(uuid + '.selected.coordinates', '');
    var orderItemInputCoordinates = $('#orderuuid' + uuid + ' .coordinates');
    orderItemInputCoordinates.val('');
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