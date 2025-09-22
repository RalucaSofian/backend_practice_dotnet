//
// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
//


// Toggle collapsible panel
function slidePanel(ids) {
    for (const id of ids) {
        if ($('#' + id).css('display') == 'none') {
            $('#' + id).slideDown('fast', function () {});
        } else {
            $('#' + id).slideUp('fast', function () {});
        }
    }
}

// Toggle "Details" button text
function changeBtnText(id) {
    var btn = document.getElementById(id);
    switch (id) {
        case "ClientDetailsBtn":
            if (btn.value == "Show Client Details") {
                btn.value = "Hide Client Details";
            } else {
                btn.value = "Show Client Details";
            }
            break;
        case "PetDetailsBtn":
            if (btn.value == "Show Pet Details") {
                btn.value = "Hide Pet Details";
            } else {
                btn.value = "Show Pet Details";
            }
            break;
        case "UserDetailsBtn":
            if (btn.value == "Show User Details") {
                btn.value = "Hide User Details";
            } else {
                btn.value = "Show User Details";
            }
            break;
        default:
            break;
    }
}


// Submit the Search String
function searchSubmit(event) {
    event.preventDefault();
    var searchForm = new FormData(event.target.form);
    var searchStrVal = searchForm.get("searchString");
    updateQueryParams({ searchString: searchStrVal });
}

// Submit the Filter Params
function filterSubmit(event) {
    event.preventDefault();
    var filterForm = new FormData(event.target.form);
    var filterValues = {};
    for (const key of filterForm.keys()) {
        filterValues[key] = filterForm.get(key);
    }

    filterValues["pageNumber"] = 1;
    updateQueryParams(filterValues);
}

// Submit the Sort Order
function orderSubmit(nextSort) {
    updateQueryParams({ sortOrder: nextSort });
}

// Submit the Page Number
function pagingSubmit(nextPage) {
    updateQueryParams({ pageNumber: nextPage });
}

// Update the full list of query params
function updateQueryParams(params) {
    // get current query params
    var queryParams = new URLSearchParams(window.location.search);
    // merge function arg into query params
    for (const key in params) {
        queryParams.set(key, params[key]);
    }
    // clear empty values
    var keysToDel = [];
    for (const key of queryParams.keys()) {
        if (!queryParams.get(key)) {
           keysToDel.push(key);
        }
    }
    for (const key of keysToDel) {
        queryParams.delete(key);
    }
    // redirect to new query params
    console.log(queryParams.toString());
    window.location.search = queryParams.toString();
}
