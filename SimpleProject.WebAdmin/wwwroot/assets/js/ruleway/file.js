var fileClass = function (el) {
    if ($(el).data("file")) {
        return $(el).data("file");
    }

    var element = $(el), $this = this, dropArea, fileInput;

    function setup() {
        dropArea = $("<div>", { class: "drop-area" }).append(
            $("<div>", { class: "empty", text: "Dosyalarınızı sürükleyip bırakın veya göz atın" }),
        );
        dropArea.insertBefore(element);

        fileInput = $("<input>", { type: "file", class: "d-none", name: element.attr("name") + "File" });
        fileInput.insertAfter(dropArea);

        if (!$app.isEmpty(element.attr("data-accept"))) {
            fileInput.attr("accept", element.attr("data-accept"));
        }

        if (!$app.isEmpty(element.attr("multiple"))) {
            fileInput.attr("multiple", element.attr("multiple"));
        }

        dropArea.on("dragover", function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropArea.addClass("drag-over");
        });
        dropArea.on("dragenter", function (e) {
            e.preventDefault();
            e.stopPropagation();
        });
        dropArea.on("dragleave", function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropArea.removeClass("drag-over");
        });

        dropArea.on("drop", function (e) {
            e.preventDefault();
            e.stopPropagation();
            dropArea.removeClass("drag-over");
            const files = e.originalEvent.dataTransfer.files;
            if (files.length) {
                if (files.length > 1 && !fileInput.is("[multiple]")) {
                    const dt = new DataTransfer();
                    dt.items.add(files[0]);
                    fileInput.prop("files", dt.files);
                }
                else {
                    fileInput.prop("files", files);
                }
                addFiles();
            }
        });

        fileInput.on('change', function (e) {
            var files = e.target.files;
            if (files && files.length) {
                addFiles();
            }
        });

        dropArea.on("click", function (e) {
            if (e.target !== dropArea.get(0) && e.target !== dropArea.find(".empty").get(0)) {
                return;
            }
            fileInput.trigger("click");
        });

        addExists();
    }


    function addFiles() {
        var files = fileInput.prop("files");
        var regexString = fileInput.attr("accept")?.replaceAll(",", "|").replaceAll("*", "\\w+");
        var regex;
        if (!$app.isEmpty(regexString)) {
            regex = new RegExp(regexString);
        }

        var removes = [];
        var previews = [];
        var names = [];
        var defaultPreviews = [];

        for (let i = 0; i < files.length; i++) {
            const file = files[i];
            const reader = new FileReader();
            reader.readAsDataURL(file);

            var preview = $("<div>", { class: "preview" }).append(
                $("<img>", { class: "rounded", src: "/assets/images/file.svg" }),
                $("<a>", { href: "javascript:;", class: "close", "data-name": file.name }),
                $("<div>", { class: "name", text: file.name.length > 10 ? "..." + file.name.substring(file.name.length - 10) : file.name })
            );
            preview.find(".close").on("click", function () {
                removeFile($(this).attr("data-name"));
            });
            
            defaultPreviews.push(preview);

            reader.onloadend = function (e) {
                if (regex && !file.type.match(regex)) {
                    removes.push(file.name);
                }
                else {
                    if (file.type.indexOf("image") !== -1) {
                        defaultPreviews[i].find("img").attr("src", e.target.result);
                    }
                    previews.push(defaultPreviews[i]);
                    names.push(file.name);
                }

                if (i == files.length - 1) {
                    if (removes.length) {
                        removeOnlyFiles(removes);
                    }
                    addPreviews(previews, names);
                }
            };
        }
    }

    function removeOnlyFiles(removes) {
        const dt = new DataTransfer();
        var files = fileInput.prop("files");
        for (const file of files) {
            if (removes.indexOf(file.name) !== -1) {
                dt.items.add(file);
            }
        }
        fileInput.prop("files", dt.files);
    }

    function addPreviews(previews, names) {
        dropArea.find(".preview:not(.exitst)").remove();
        for (const preview of previews) {
            dropArea.prepend(preview);
        }
        if (!dropArea.find(".exists").length) {
            element.val(names.join(","));
        }
    }

    function removeFile(name) {
        const dt = new DataTransfer();
        var files = fileInput.prop("files");
        for (const file of files) {
            if (file.name != name) {
                dt.items.add(file);
            }
        }
        fileInput.prop("files", dt.files);
        if (!dt.files.length) {
            dropArea.find(".preview:not(.exitst)").remove();
        }
        addFiles();
    }

    var addExists = function () {
        if ($app.isEmpty(element.val())) {
            return;
        }

        var path = element.val();
        var fileName = path.split('/').pop();
        var ext = fileName.split('.').pop().toLowerCase();
        var images = ["jpg", "png", "gif", "webp", "tiff", "bmp", "svg", "jpeg"];

        var preview = $("<div>", { class: "preview exists" }).append(
            $("<img>", { class: "rounded", src: "/assets/images/file.svg" }),
            $("<a>", { href: "javascript:;", class: "close" }),
            $("<div>", { class: "name", text: fileName.length > 10 ? "..." + fileName.substring(fileName.length - 10) : fileName })
        );

        if (images.indexOf(ext) !== -1) {
            preview.find("img").attr("src", path);
        }

        preview.find(".close").on("click", function () {
            element.val("");
            $(this).parents(".preview:first").remove();
            addFiles();
        });

        dropArea.prepend(preview);
    }

    this.imagePath = function (path) {
        fileInput.val(null);
        dropArea.find(".preview").remove();
        element.val(path);

        addExists();
    }

    element.data("file", this);
    setup();
}

function $file(el) {
    var file = $(el).data("file");
    if (file) {
        return file;
    }
    return new fileClass(el);
}

$(function () {
    $("[data-file-upload]").each(function () {
        $file(this);
    });
    $app.ajaxComplete.push(function () {
        $("[data-file-upload]").each(function () {
            $file(this);
        });
    });
});