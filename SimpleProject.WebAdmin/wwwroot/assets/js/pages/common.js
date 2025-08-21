function getConfigField(data, inputSelector) {
    var field = $("<div>", { class: "row mb-3" }).append(
        $("<label>", { class: "col-md-2 offset-lg-1 col-form-label text-md-end", html: data.fieldType == 4 ? "&nbsp;" : data.label }),
        $("<div>", { class: "col-md-10 col-lg-8" })
    );

    if (data.fieldType == 7) {
        field.find("div").append(
            $("<select>", { class: "select2" }).append(
                $("<option>", { text: $lang.selectEmpty, value: "" })
            )
        );
        if (data.options) {
            var input = field.find(":input");
            $.each(JSON.parse(data.options), function (index, value) {
                input.append(
                    $("<option>", { text: value, value: value })
                );
            });
        }
    }
    else if (data.fieldType == 4) {
        field.find("div").append(
            $("<div>", { class: "form-check form-control border-0" }).append(
                $("<input>", { class: "form-check-input", type: "checkbox", value: "true", id: "_settings_input_" + data.name }),
                $("<label>", { class: "form-check-label", for: "_settings_input_" + data.name, text: data.label })
            )
        );
    }
    else if (data.fieldType == 8) {
        field.find("div").append(
            $("<textarea>", { class: "form-control"})
        );
    }
    else {
        field.find("div").append(
            $("<input>", { class: "form-control", type: "text" })
        );
        var input = field.find(":input");
        if (data.fieldType == 2) {
            input.addClass("integer");
        }
        else if (data.fieldType == 3) {
            input.addClass("float");
        }
        else if (data.fieldType == 5) {
            input.addClass("date-time");
        }
        else if (data.fieldType == 6) {
            input.addClass("date");
        }
    }

    if (data.fieldType == 4) {
        field.find(":input").prop("checked", data.value == "true");
    }
    else {
        field.find(":input").val(data.value);
    }

    if (inputSelector) {
        field.find(":input").attr("data-settings-name", data.name).on("change", function () {
            var values = [];
            $("[data-settings-name]").each(function (si, s) {
                values.push({ Name: $(s).attr("data-settings-name"), Value: $(s).val() });
            });
            $(inputSelector).val(JSON.stringify(values));
        });
    }
    return field;
}

function changeGridUrlParams(key, value, exeptions) {
    $(".ruleway-grid").each(function () {
        const grid = $grid(this)
        if (exeptions && exeptions.indexOf(grid.id)) {
            return;
        }
        grid.readUrl($app.changeQueryParam(grid.readUrl(), key, value));
    });
}

function bindEditTabEvent(fnDone) {
    $(".tab-btn").addClass("d-none");
    $(".tab-btn.tab-1").removeClass("d-none");

    $("[data-bs-toggle='tab']").on("shown.bs.tab", function (event) {
        var tab = $(event.target).attr("href");
        triggerTabEvent(tab);
        if (fnDone && typeof fnDone === "function") {
            fnDone(tab);
        }
    });
}

function triggerTabEvent(tab) {
    $(".tab-btn").addClass("d-none");
    $(".tab-btn." + tab.replace("#", "")).removeClass("d-none");
    if (!$(tab).is(".tab-loaded")) {
        $(tab).addClass("tab-loaded");
        $(tab + " .ruleway-grid").each(function () {
            $grid(this).load();
        });
    }
    else {
        $(tab + " .ruleway-grid").each(function () {
            $grid(this).setAutoHeight();
        });
    }
    adjustHeaderMenu();
}

function orderTableInputs(table, attribute) {
    table.find("> tr").each(function (i, e) {
        $(e).find(":input").each(function (i1, e1) {
            var input = $(e1);
            var inputName = input.attr("name");
            if (!inputName || inputName.indexOf(attribute) != 0) {
                return;
            }
            var index = inputName.indexOf("]", attribute.length);
            if (index >= 0) {
                input.attr("name", attribute + "[" + i + "]" + inputName.substring(index + 1))
            }
        });

        $(e).find("button[data-field]").each(function (i1, e1) {
            var button = $(e1);
            var fieldName = button.attr("data-field");
            if (fieldName == attribute || fieldName.indexOf(attribute) != 0) {
                return;
            }

            var index = fieldName.indexOf("]", attribute.length);
            if (index >= 0) {
                button.attr("data-field", attribute + "[" + i + "]" + fieldName.substring(index + 1))
            }
        });
    });
}

function adjustHeaderMenu() {
    if (!$("#header .header-content-left .nav").length) {
        return;
    }

    if (!$("#header .header-content-left .nav .nav-item.dropdown").length) {
        $("#header .header-content-left .nav").append(
            $("<li>", { class: "nav-item dropdown", role: "presentation" }).append(
                $("<a>", { class: "nav-link dropdown-toggle", "data-bs-toggle": "dropdown", href: "javascript:;", role: "button" }).append(
                    $lang.other
                ),
                $("<ul>", { class: "dropdown-menu" })
            )
        );
    }

    var width = $("#header .main-header-container").width()
        - $("#header .header-content-right").outerWidth()
        - $("#header .header-content-left .header-element:eq(0)").outerWidth()
        - $("#header .header-content-left .header-element:eq(1)").outerWidth();
    - 10;

    var current = $("#header .header-content-left .nav .nav-item.dropdown").outerWidth() + 10;
    var moved = false;
    $("#header .header-content-left .nav .nav-item.dropdown ul").empty();
    $("#header .header-content-left .nav .nav-item:not(.dropdown)").removeClass("d-none").each(function () {
        var item = $(this);
        current += (item.outerWidth() + 10);
        if (current >= width) {
            var disabled = item[0].children[0].classList.value.includes("disabled");
            var clone = $("<li>").append(
                $("<a>", { class: "nav-link dropdown-item text-nowrap", href: "javascript:;" }).append(
                    item.find("a").html()
                )
            );
            if (disabled == true) {
                clone = $("<li>").append(
                    $("<a>", { class: "nav-link disabled dropdown-item text-nowrap", href: "javascript:;" }).append(
                        item.find("a").html()
                    )
                );
            }
            $("#header .header-content-left .nav .nav-item.dropdown ul").append(clone);
            item.addClass("d-none");
            clone.find("a").on("click", function () {
                item.find("a").get(0).click();
            });
            moved = true;
        }
    });

    if (!moved) {
        $("#header .header-content-left .nav .nav-item.dropdown").addClass("d-none");
    }
    else {
        $("#header .header-content-left .nav .nav-item.dropdown").removeClass("d-none");
    }
}

$(function () {
    adjustHeaderMenu();
    $(window).on("resize", adjustHeaderMenu);
});