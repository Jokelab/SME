<?php
$name = $_GET['name'];
$query = "SELECT * FROM users WHERE name='" . $name . "'";
mysql_query($query);
?>