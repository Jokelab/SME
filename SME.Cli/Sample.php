<html>
<?php
$name = $_GET['name'];
$msg = 'Welcome '.$name;
?>
<head>
<title><?php echo($name); /* XSS 1 */ ?></title>
</head>
<body>
<?php 
	echo "Hello! " . $msg; /* XSS 2 */ 
?>
</body>
</html>