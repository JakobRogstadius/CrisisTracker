<?php
function printTitle($pageTitle) {
    echo "<title>$pageTitle - AIDR</title>";
}
?>
<!DOCTYPE html>
<html>
    <head>
        <meta charset="utf-8">
        <script type="text/javascript" src="../jquery-1.9.1.min.js"></script>
        <script>
            function validateInput() {
                if ($("#options").find("input:checked").size() === $("#options").find("ul").size()) {
                    return true;
                } else {
                    alert("Please select one option for each label category");
                    return false;
                }
            }
            function goto(url) {
                window.location=url;
            }
            function confirmGoto(question, url) {
                if (confirm(question)) {
                    goto(url);
                }
            }
        </script>
        <link rel="stylesheet" type="text/css" href="admin.css" media="screen" />
