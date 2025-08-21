function editRecord() {
    $app.gotoUrl("/adminusers/edit/" + $(this).attr("data-id"));
}

function deleteRecord() {
    var id = $(this).attr("data-id");
    $app.confirm($lang.deleteConfirm, function () {
        $app.callJx("/adminusers/delete", "body", { id: id }, function (result) {
            $grid(".ruleway-grid").load();
        });
    });
}

function showUpload() {
    $app.showUpload('/adminusers/upload', function () {
        $grid(".ruleway-grid").load();
    });
}