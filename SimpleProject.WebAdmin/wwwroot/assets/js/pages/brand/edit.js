function saveBrand(fnDone) {
    var form = $("#frmBrand");
    var id = form.find(":input[name='Id']").val();
    if (form.valid()) {
        $app.postJx(form.attr("action"), "body", form, function (result) {
            if (fnDone && typeof fnDone === "function") {
                fnDone.call(result);
                return;
            }
            $app.toast("success", $lang.recordSaved, function () {
                if (id == "0") {
                    if (history && history.replaceState) {
                        form.find(":input[name='Id']").val(result.data.id);
                        history.replaceState({}, document.title, "/brands/edit/" + result.data.id);
                    }
                    else {
                        $app.gotoUrl("/brands/edit/" + id);
                    }
                }

                $file("#frmBrand :input[name='Image']").imagePath(result.data.image);
                $("#frmBrand :input[name='Thumbnail']").val(result.data.thumbnail);
                form.find(":input[name='DisplayOrder']").val(result.data.displayOrder);
            });
        });
    }
}

$(function () {
    $("#frmBrand").validate();
});