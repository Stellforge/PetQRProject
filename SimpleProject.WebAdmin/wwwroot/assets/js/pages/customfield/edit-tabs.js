function editRecord(grid) {
    var modal = $app.openLinkModal("/customfieldoptions/edit", { id: $(this).attr("data-id"), customFieldId: $("#frmCustomField :input[name='Id']").val(), modal: true }, function () {
        saveCustomFieldOption(function () {
            $app.toast("success", $lang.recordSaved);
            bootstrap.Modal.getInstance(modal).hide();
            $grid("#" + grid).load();
        });
    });
}

function deleteRecord(grid) {
    var id = $(this).attr("data-id");
    $app.confirm($lang.deleteConfirm, function () {
        $app.callJx("/customfieldoptions/delete", "body", { id: id }, function (result) {
            $grid("#" + grid).load();
        });
    });
}

$(function () {
    bindEditTabEvent();
});