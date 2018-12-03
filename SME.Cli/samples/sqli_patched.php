<?php
$name = mysql_real_escape_string($_GET['name']);
$query = "SELECT * FROM users WHERE name='" . $name . "'";
mysql_query($query);
?>