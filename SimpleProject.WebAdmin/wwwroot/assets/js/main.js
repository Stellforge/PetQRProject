$(function () {
    $("#loader").addClass("d-none");

    $("[data-help-page]").on("click", function () {
        $('#off-canvas .offcanvas-body').html('<iframe src="https://helpcenter.ruleway.com/' + $('[data-help-page]').attr('data-help-page') + '" width="100%" height="100%" frameborder="0"></iframe>');
        $("#offcanvasRightLabel").text($('[data-help-title]').attr('data-help-title'));
        $("#off-canvas").css("width", "95%");
        
    });

    $("#off-canvas").on('hidden.bs.offcanvas', function () {
        $("body").removeClass("overflow-hidden");
        $("#off-canvas").css("width", "");
    });

    $("#off-canvas").on('shown.bs.offcanvas', function () {
        $("body").addClass("overflow-hidden");
    });

    $(".scrollToTop").on("click", function () {
        $("html, body").animate({ scrollTop: 0 }, "fast");
    });
    $(window).on("scroll", function () {
        if (window.scrollY > 100) {
            $(".scrollToTop").removeClass("d-none").addClass("d-flex");
        }
        else {
            $(".scrollToTop").removeClass("d-flex").addClass("d-none");
        }
    });

    new SimpleBar(document.getElementById('sidebar-scroll'), { autoHide: true });
    const notificationScroll = document.getElementById("header-notification-scroll");
    if (notificationScroll) {
        new SimpleBar(notificationScroll, { autoHide: true });
    }

    if (localStorage.getItem("darktheme")) {
        $("html").attr("data-theme-mode", "dark");
        $("html").attr("data-menu-styles", "dark");
        $("html").attr("data-header-styles", "dark");
    }

    $(".layout-setting").on("click", function () {
        if ($("html").attr("data-theme-mode") === "dark") {
            $("html").attr("data-theme-mode", "light");
            $("html").attr("data-header-styles", "light");
            $("html").attr("data-menu-styles", "dark");

            $("html").removeAttr("data-bg-theme");
            localStorage.removeItem("darktheme");
        } else {
            $("html").attr("data-theme-mode", "dark");
            $("html").attr("data-header-styles", "dark");
            $("html").attr("data-menu-styles", "dark");

            localStorage.setItem("darktheme", "true");
        }
    });

    componentTooltip();
    componentPopover();
    componentSelect2();
    componentAutoNumeric();
    componentDate();
    componentInputMask();
});

// Tooltip
function componentTooltip() {
    $('[data-bs-popup="tooltip"]').each(function (i, popup) {
        new bootstrap.Tooltip(popup);
    });
};

// Popover
function componentPopover() {
    $('[data-bs-popup="popover"]').each(function (i, popup) {
        new bootstrap.Popover(popup);
    });
};

// Select2
function componentSelect2() {
    $("select.select2").each(function (i, select) {
        if ($(select).hasClass("select2-done")) {
            return;
        }
        var opt = {
            width: "100%"
        };

        var classList = Array.from(select.classList).filter(a => a != "select2");
        if ($(select).is("[data-autocomplete]")) {
            if (!$(select).is("[multiple]")) {
                $(select).attr("multiple", "multiple");
                opt["maximumSelectionLength"] = 1;
                opt["language"] = {
                    maximumSelected: function (e) {
                        return $(select).find("option:selected").text();
                    }
                };
                $(select).addClass("select2-tags-simple");
            }
            opt["tags"] = true;
        }
        if ($(select).parents(".modal:first").length) {
            opt.dropdownParent = $(select).parents(".modal:first");
        }
        
        if ($(select).attr("data-url")) {
            opt.ajax = {
                url: function () { return $(select).attr("data-url"); },
                method: "POST",
                data: function (params) {
                    var query = {
                        q: params.term,
                        page: params.page || 1
                    }
                    return query;
                }
            };
            opt.templateSelection = function (data, container) {
                if (!$app.isEmpty(data.extra)) {
                    $(data.element).attr('data-extra', data.extra);
                }
                if ($(data.element).parents("select:first").attr("data-autocomplete") == "true")
                {
                    if (!$(data.element).is("[data-select2-tag='true']")) {
                        $(data.element).parents("select:first").attr('data-extra-id', data.id);
                    }
                    else {
                        $(data.element).parents("select:first").attr('data-extra-id', "0");
                    }
                }
                return data.text;
            }
            $(select).select2(opt);
        }
        else {
            $(select).select2(opt);
        }

        $(select).addClass("select2-done");

        if (classList.length) {
            $(select).next(".select2").addClass(classList.join(" "));
        }

        $(select).on('select2:opening', function (e) {
            if ($(this).attr('readonly') == 'readonly') {
                e.preventDefault();
                $(this).select2('close');
                return false;
            }
        });
    });
};

function destroyComponentSelect2(element) {
    $(element).each(function () {
        const item = $(this);
        if (item.hasClass("select2-done")) {
            item.removeClass("select2-done").removeClass("select2").select2("destroy");
        }
    });
}

// autoNumeric
function componentAutoNumeric() {
    if ($().autoNumeric) {
        $('.integer:not(.num-done)').autoNumeric('init', { digitGroupSeparator: "", decimalPlacesOverride: 0, minimumValue: -9999999999999, maximumValue: 9999999999999 }).addClass("num-done");
        $('.float:not(.num-done)').each(function () {
            var decimalPlaces = 2;
            if ($(this).is("[data-decimal-places]")) {
                decimalPlaces = parseInt($(this).attr("data-decimal-places"));
            }
            $(this).autoNumeric('init', { digitGroupSeparator: $numberFormat.group, decimalCharacter: $numberFormat.decimal, decimalPlacesOverride: decimalPlaces }).addClass("num-done");
        });
    }
};

function destroyComponentAutoNumeric(element) {
    $(element).each(function () {
        const item = $(this);
        if (item.hasClass("num-done")) {
            item.removeClass("num-done").removeClass("integer").removeClass("float").autoNumeric("destroy");
        }
    });
}

// date
function componentDate() {
    if ($().flatpickr) {
        if ($lang.code != "en") {
            flatpickr.localize(flatpickr.l10ns[$lang.code]);
        }
        $('.date:not(.date-done)').each(function () {
            if (!$(this).attr("data-rule-required")) {
                $(this).wrap($("<div>", { class: "input-group" }));
                $(this).parent().append(
                    $("<a>", { href: "javascript:;", class: "btn btn-light-outline", style:"position:absolute;right:0", title: $lang.clear, "onclick":"clearInputGroup(this)" }).append(
                        $("<i>", { class: "ri-close-large-line" })
                    )
                );
            }

            $(this).addClass("date-done").flatpickr({
                altInput: true,
                enableTime: false,
                altFormat: "d-m-Y",
                dateFormat: "Y-m-d",
            });
        });
        

        $('.date-time:not(.date-done)').each(function () {
            if (!$(this).attr("data-rule-required")) {
                $(this).wrap($("<div>", { class: "input-group" }));
                $(this).parent().append(
                    $("<a>", { href: "javascript:;", class: "btn btn-light-outline", style: "position:absolute;right:0", title: $lang.clear, "onclick": "clearInputGroup(this)" }).append(
                        $("<i>", { class: "ri-close-large-line" })
                    )
                );
            }

            $(this).addClass("date-done").flatpickr({
                altInput: true,
                enableTime: true,
                time_24hr: true,
                altFormat: "d-m-Y H:i",
                dateFormat: "Y-m-d H:i",
            });
        });

        $('.time:not(.date-done)').each(function () {
            if (!$(this).attr("data-rule-required")) {
                $(this).wrap($("<div>", { class: "input-group" }));
                $(this).parent().append(
                    $("<a>", { href: "javascript:;", class: "btn btn-light-outline", style: "position:absolute;right:0", title: $lang.clear, "onclick": "clearInputGroup(this)" }).append(
                        $("<i>", { class: "ri-close-large-line" })
                    )
                );
            }

            $(this).addClass("date-done").flatpickr({
                enableTime: true,
                noCalendar: true,
                dateFormat: "H:i",
                time_24hr: true
            });
        });
    }
};

function destroyComponentDate(element) {
    $(element).each(function () {
        const item = $(this);
        if (item.hasClass("date-done")) {
            item.removeClass("date-done").removeClass("date").removeClass("time").removeClass("date-time");
            item.get(0)._flatpickr.destroy();
            const parent = item.parent();
            if (parent.hasClass("input-group")) {
                parent.after(item);
                parent.remove()
            }
        }
    });
}

function componentInputMask() {
    $(":input[data-inputmask]").inputmask();
}

function destroyComponentInputMask(element) {
    $(element).inputmask("remove").removeAttr("data-inputmask");
}

function openFullscreen() {
    if (!document.fullscreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement) {
        if (document.documentElement.requestFullscreen) {
            document.documentElement.requestFullscreen();
        }
        else if (document.documentElement.webkitRequestFullscreen) {
            document.documentElement.webkitRequestFullscreen();
        }
        else if (elem.msRequestFullscreen) {
            document.documentElement.msRequestFullscreen();
        }
    }
    else {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        }
        else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        }
        else if (document.msExitFullscreen) {
            document.msExitFullscreen();
        }
    }
}

document.addEventListener("fullscreenchange", function () {
    if (document.fullscreenElement || document.webkitFullscreenElement || document.msFullscreenElement) {
        // Update icon for fullscreen mode
        $(".full-screen-close").removeClass("d-none").addClass("d-block");
        $(".full-screen-open").removeClass("d-block").addClass("d-none");
    }
    else {
        // Update icon for non-fullscreen mode
        $(".full-screen-close").removeClass("d-block").addClass("d-none");
        $(".full-screen-open").removeClass("d-none").addClass("d-block");
    }
});

function clearInputGroup(btn) {
    var input = $(btn).parents(".input-group:first").find(":input");
    if (input.is("select")) {
        input.clearSelect2();
    }
    else {
        input.val("").trigger("change");
    }
}

jQuery.fn.clearSelect2 = function () {
    $.each(this, function (i, e) {
        if ($(e).is("[data-url]")) {
            $(e).empty().append(
                $("<option>", { value: "", text: $lang.selectEmpty })
            ).trigger("change");
        }
        else {
            $(e).find("option:first").get(0).selected = true;
            $(e).trigger("change");
        }
    });
    return this;
};