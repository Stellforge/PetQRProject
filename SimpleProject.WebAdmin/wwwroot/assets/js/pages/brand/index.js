function editRecord() {
    $app.gotoUrl("/brands/edit/" + $(this).attr("data-id"));
}

function deleteRecord() {
    var id = $(this).attr("data-id");
    $app.confirm($lang.deleteConfirm, function () {
        $app.callJx("/brands/delete", "body", { id: id }, function (result) {
            $grid(".ruleway-grid").load();
        });
    });
}

function showUpload() {
    $app.showUpload('/brands/upload', function () {
        $grid(".ruleway-grid").load();
    });
}