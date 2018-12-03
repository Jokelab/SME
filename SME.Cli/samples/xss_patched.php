<html>
<?php
$name = htmlspecialchars($_GET['name']);
$msg = 'Welcome '.$name;
?>
<head>
<title><?php echo($name); /* XSS 1 */ ?></title>
</head>
<body>
<?php 
	echo "Hello xss! " . $msg; /* XSS 2 */ 
?>
</body>
</html>