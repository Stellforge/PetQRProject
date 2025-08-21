var $template = function () {
    var badges = [
        "<span class='badge bg-primary-transparent'>{0}</span>",
        "<span class='badge bg-primary1-transparent'>{0}</span>",
        "<span class='badge bg-primary2-transparent'>{0}</span>",
        "<span class='badge bg-primary3-transparent'>{0}</span>",
        "<span class='badge bg-secondary-transparent'>{0}</span>",
        "<span class='badge bg-info-transparent'>{0}</span>",
        "<span class='badge bg-success-transparent'>{0}</span>",
        "<span class='badge bg-dark-transparent'>{0}</span>",
        "<span class='badge bg-warning-transparent'>{0}</span>",
        "<span class='badge bg-danger-transparent'>{0}</span>"
    ];
    var statuses =
        [
            "<span class='badge bg-danger-transparent'>{0}</span>",
            "<span class='badge bg-primary-transparent'>{0}</span>",
            "<span class='badge bg-secondary-transparent'>{0}</span>",
            "<span class='badge bg-success-transparent'>{0}</span>",
            "<span class='badge bg-info-transparent'>{0}</span>",
            "<span class='badge bg-dark-transparent'>{0}</span>",
            "<span class='badge bg-warning-transparent'>{0}</span>",
            "<span class='badge bg-primary1-transparent'>{0}</span>",
            "<span class='badge bg-primary2-transparent'>{0}</span>",
            "<span class='badge bg-primary3-transparent'>{0}</span>",
        ];
    var availabilities =
        [
            "<span class='badge bg-primary-transparent'>{0}</span>",
            "<span class='badge bg-primary3-transparent'>{0}</span>",
            "<span class='badge bg-danger-transparent'>{0}</span>",
            "<span class='badge bg-secondary-transparent'>{0}</span>",
            "<span class='badge bg-success-transparent'>{0}</span>",
            "<span class='badge bg-info-transparent'>{0}</span>",
            "<span class='badge bg-dark-transparent'>{0}</span>",
            "<span class='badge bg-warning-transparent'>{0}</span>",
            "<span class='badge bg-primary1-transparent'>{0}</span>",
            "<span class='badge bg-primary2-transparent'>{0}</span>",
        ];
    var logTypes =
        [
            "<span class='badge bg-primary-transparent'>{0}</span>",
            "<span class='badge bg-secondary-transparent'>{0}</span>",
            "<span class='badge bg-danger-transparent'>{0}</span>"
        ];

    return {
        getTemplateFn: function (name) {
            if (name.indexOf(".") >= 0) {
                var temps = name.split(".");
                var temp = window[temps[0]];
                for (var i = 1; i < temps.length; i++) {
                    if (temp && temp[temps[i]]) {
                        temp = temp[temps[i]];
                    }
                    else {
                        break;
                    }
                }

                if (temp && typeof temp === "function") {
                    return temp;
                }
            }
            else if (window[name] && typeof window[name] === "function") {
                return window[name];
            }
        },
        actions: function (data, index) {
            return $("<div>", { class: "btn-list text-nowrap" }).append(
                $("<button>", { class: "btn btn-sm btn-icon btn-primary-light grid-edit", "data-id": data[index] }).append(
                    $("<i>", { class: "ri-edit-box-line" })
                ),
                $("<button>", { class: "btn btn-sm btn-icon btn-danger-light grid-delete", "data-id": data[index] }).append(
                    $("<i>", { class: "ri-delete-bin-line" })
                )
            );
        },
        edit: function (data, index) {
            return $("<div>", { class: "btn-list text-nowrap" }).append(
                $("<button>", { class: "btn btn-sm btn-icon btn-primary-light grid-edit", "data-id": data[index] }).append(
                    $("<i>", { class: "ri-edit-box-line" })
                )
            );
        },
        delete: function (data, index) {
            return $("<div>", { class: "btn-list text-nowrap" }).append(
                $("<button>", { class: "btn btn-sm btn-icon btn-danger-light grid-delete", "data-id": data[index] }).append(
                    $("<i>", { class: "ri-delete-bin-line" })
                )
            );
        },
        changeApprove: function (data, index) {
            var button = $("<button>", { class: "btn btn-sm btn-icon btn-primary-light grid-change-approve", "data-id": data[index] }).append(
                $("<i>", { class: "ri-check-double-line" })
            );

            button.on("click", function () {
                var row = $(this).parents("tr:first");
                if (typeof window["changeApproveRecord"] == "function") {
                    window["changeApproveRecord"].call(this);
                }
            });

            return $("<div>", { class: "btn-list text-nowrap" }).append(button);
        },
        status: function (data, index) {
            if (data[index] && data[index].length > 1) {
                return $app.format(statuses[data[index][0] % statuses.length], data[index][1]);
            }
            return "";
        },
        badge: function (data, index) {
            if (data[index] && data[index].length > 1) {
                return $app.format(badges[data[index][0] % badges.length], data[index][1]);
            }
            return "";
        },
        availability: function (data, index) {
            if (data[index] && data[index].length > 1) {
                return $app.format(availabilities[data[index][0] % availabilities.length], data[index][1]);
            }
            return "";
        },
        logType: function (data, index) {
            if (data[index] && data[index].length > 1) {
                return $app.format(logTypes[data[index][0] % logTypes.length], data[index][1]);
            }
            return "";
        },
        dateTime: function (data, index) {
            if ($app.isEmpty(data[index])) {
                return "";
            }
            var date = new Date(data[index]);
            return date.getDate().toString().padStart(2, '0') + "." + (date.getMonth() + 1).toString().padStart(2, '0') + "." + date.getFullYear()
                + " " + date.getHours().toString().padStart(2, '0') + ":" + date.getMinutes().toString().padStart(2, '0');
        },
        date: function (data, index) {
            if ($app.isEmpty(data[index])) {
                return "";
            }
            var date = new Date(data[index]);
            return date.getDate().toString().padStart(2, '0') + "." + (date.getMonth() + 1).toString().padStart(2, '0') + "." + date.getFullYear();
        },
        image: function (data, index) {
            var val = data[index];
            if ($app.isEmpty(val)) {
                return $("<span>", { class: "avatar avatar-md avatar-square bg-light" }).append(
                    $("<img>", { src: "/assets/images/no-image.svg", class: "w-100 h-100" })
                );
            }
            else {
                return $("<span>", { class: "avatar avatar-md avatar-square bg-light" }).append(
                    $("<img>", { src: val, class: "w-100 h-100" })
                );
            }
        },
        bool: function (data, index) {
            if (data[index]) {
                return $("<i>", { class: "ri-checkbox-line fs-5 lh-1" });
            }
            else {
                return $("<i>", { class: "ri-checkbox-blank-line fs-5 lh-1" });
            }
        },
        number: function (data, index) {
            var val = data[index];
            if (val != undefined && val != null && val != "") {
                return val.toLocaleString($lang.code, { minimumFractionDigits: 2 });
            }
            return "";
        },
        numberInput: function (data, index) {
            var idIdx = this.getFieldIndex("Id");
            if (idIdx >= 0) {
                var id = data[idIdx];
                return $("<div>", { class: "" }).append(
                    $("<input>", { type: "text", class: "form-control integer", value: data[index], id: id })
                );
            }
            return "";
        },
        textInput: function (data, index) {
            var idIdx = this.getFieldIndex("Id");
            if (idIdx >= 0) {
                var id = data[idIdx];
                return $("<div>", { class: "" }).append(
                    $("<input>", { type: "text", class: "form-control", value: id, id: data[index] + "_" + id })
                );
            }
            return "";
        },
        integer: function (data, index) {
            var val = data[index];
            if (val != undefined && val != null && val != "") {
                return val.toLocaleString($lang.code);
            }
            return "";
        },
        checkBox: function (data, index) {
            var idIdx = this.getFieldIndex("Id");
            if (idIdx >= 0) {
                var id = data[idIdx];
                return $("<div>", { class: "form-check" }).append(
                    $("<input>", { type: "checkbox", class: "form-check-input check-col", value: id, id: data[index] + "_" + id })
                );
            }
            return "";
        },
        parentCategory: function (data, index) {
            var val = data[index];
            if ($app.isEmpty(val)) {
                return $lang.mainPage;
            }
            return val;
        },
        allVariants: function (data, index) {
            var val = data[index];
            if (!$app.isEmpty(val)) {
                return $("<i>", { class: "ri-checkbox-line fs-5 lh-1" });
            }
            return $("<i>", { class: "ri-checkbox-blank-line fs-5 lh-1" });
        },
        imageWithType: function (data, index) {
            if (data[index] && data[index].length > 1) {
                if (data[index][1] == 0) {
                    return $("<a>", { href: "javascript:;", onclick: "$template.showImage(this)" }).append(
                        $("<span>", { class: "avatar avatar-md avatar-square bg-light" }).append(
                            $("<img>", { src: data[index][0], class: "w-100 h-100" })
                        )
                    );
                }
                else {
                    return $("<a>", { href: data[index][0], text: data[index][0], target: "_blank" });
                }
            }
            return "";
        },
        showImage: function (btn) {
            $app.showModal("success", undefined, "<img class='img-fluid rounded' src='" + $(btn).find("img").attr("src") + "' />").find(".modal-body").addClass("text-center");
        }
    }
}();