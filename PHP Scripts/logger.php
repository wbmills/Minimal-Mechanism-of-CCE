<?php

if (!isset($vist_page))
{
	$vist_page     =   "logger.php";
}
$user_agent     =   $_SERVER['HTTP_USER_AGENT'];

function getIP() 
{ 
	if (isset($_SERVER)) 
	{ 
		if (isset($_SERVER['HTTP_X_FORWARDED_FOR'])) 
		{ 
			$realip = $_SERVER['HTTP_X_FORWARDED_FOR']; 
		} 
		else
			if (isset($_SERVER['HTTP_CLIENT_IP'])) 
			{ 
				$realip = $_SERVER['HTTP_CLIENT_IP']; 
			} 
			else 
			{ 
				$realip = $_SERVER['REMOTE_ADDR']; 
			} 
		} 
	else 
	{
		if (getenv("HTTP_X_FORWARDED_FOR")) 
		{ 
			$realip = getenv( "HTTP_X_FORWARDED_FOR"); 
		} 
		else
			if (getenv("HTTP_CLIENT_IP")) 
			{ 
				$realip = getenv("HTTP_CLIENT_IP"); 
			}
			else
			{ 
				$realip = getenv("REMOTE_ADDR"); 
	} 
} 
return $realip; 
}
session_start();
if (!isset($_SESSION[$vist_page]) || $_SESSION[$vist_page]!=1)
{
	$_SESSION[$vist_page]=1;
	$ip = getIP();

	$site_refer = "";
	if (isset($_SERVER['HTTP_REFERER'])) {
		$site_refer = $_SERVER['HTTP_REFERER'];
	}

	if ($site_refer == ""){
		$site = "direct connection";
	} else{
		$site = $site_refer;
	}

	$details = json_decode(file_get_contents("http://ipinfo.io/{$ip}"));
	$country = $details->country;
	PostToDatabase("INSERT INTO ip_info (participant_id, ip_address, country, user_agent) 
	VALUES('$participant_id', '$ip', '$country' '$user_agent');");
}
?>
