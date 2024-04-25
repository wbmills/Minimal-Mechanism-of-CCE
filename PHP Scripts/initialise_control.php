<?php
date_default_timezone_set("America/New_York");
// Get variables from URL
$participant_id = $_GET["participant_id"]; // random string
$consent = $_GET["consent"]; // 1 or 0
$age = $_GET["age"]; // (years) int
$is_human = $_GET["is_human"]; // (participant/AI) 1 or 0
$hash = $_GET["hash"]; // hash code
$given_con = $_GET["given_con"];
$gender = $_GET["gender"];


// MySQL Configuration
$hostname = 'premium149.web-hosting.com';
$username = 'willktyg_user';
$password = 'WillMills3451';
$database = 'willktyg_cce';
$secretKey = "WillMills3451";

$realHash = hash('SHA512', $participant_id . $secretKey, false);
$realHash = strtoupper($realHash);
if ($hash != $realHash){
    exit("'Non-Matching Hash Code >:('");
}

function reduceMinutesToTime($time, $minutesToTakeAway) {
    $dateTime = DateTime::createFromFormat('Y-m-d H:i:s', $time);
    $dateTime->modify("-{$minutesToTakeAway} minutes");
    return $dateTime->format('Y-m-d H:i:s');
}

function GetUniqueID2() {
    // Generate 16 bytes of random data
    $data = random_bytes(16);

    // Set the version to 4 (random)
    $data[6] = chr(ord($data[6]) & 0x0f | 0x40);

    // Set the variant to RFC 4122
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80);

    // Convert the binary UUID data to a hexadecimal string
    $abc = vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
    $new_uuid = str_replace("-", "", $abc);
    return $new_uuid;
}

// post to SQL and get back information from table
function PostAndGetFromDatabase($sql) {
    global $hostname, $username, $password, $database;
    
    // Establish a database connection
    $dbh = new PDO("mysql:host=$hostname;dbname=$database", $username, $password);
    
    try {
        // ErunData$runDataecute the SQL query
        $statement = $dbh->query($sql);
        
        // Set the fetch mode to associative array
        $statement->setFetchMode(PDO::FETCH_ASSOC);
        
        // Fetch all rows
        $result = $statement->fetchAll();
        
        // Return the result
        return $result;
    } catch (PDOException $e) {
        // Error occurred during erunData$runDataecution
        throw new Exception('error: ' . $e->getMessage());
    }
}

// post to database, no get
function PostToDatabase($sql) {
    global $hostname, $username, $password, $database;
    $dbh = new PDO("mysql:host=$hostname;dbname=$database", $username, $password);
    try {
        $statement = $dbh->query($sql);
        $result = $statement->fetch();

    } catch(Exception $e) {
        throw new Exception('Error: ' . $e->getMessage());
    }
}


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

function logger(){
    global $participant_id;
    if (!isset($vist_page))
    {
        $vist_page     =   "initialise.php";
    }
    $user_agent     =   $_SERVER['HTTP_USER_AGENT'];

    $ip = getIP();

    $details = null;  //json_decode(file_get_contents("http://ipinfo.io/{$ip}"));
    if ($details != null){
        $country = $details->country;
        PostToDatabase("INSERT INTO ip_info (participant_id, ip_address, country, user_agent) 
        VALUES ('$participant_id', '$ip', '$country', '$user_agent');");
    }
}

function GetCurrentTime(){
    $microtime = microtime(true);
    $dateTime = DateTime::createFromFormat('U.u', $microtime);
    $dateTime->setTimezone(new DateTimeZone('America/Detroit'));
    return $dateTime->format('Y-m-d H:i:s');
}

function add_session($min_index, $is_full){
    $_t = reduceMinutesToTime(GetCurrentTime(), 25);
    $sql3 = "SELECT GROUP_CONCAT(participant_id) AS participant_ids FROM participant WHERE condition_part_1 = $min_index AND condition_part_2 = 0 and CAST(time_joined as time) > '$_t';";
    $all_ids = PostAndGetFromDatabase($sql3);
    var_dump($all_ids);
    if (count($all_ids) > 0){
        foreach($all_ids as $i){
            // convert to json
            $playerIds = explode(",", $i["participant_ids"]);
            foreach ($playerIds as $_id){
                PostToDatabase("UPDATE participant SET condition_part_2 = 1 WHERE participant_id = '$_id';");
            }
            $playerIds = array_map('strval', $playerIds);
            $outputJSON = json_encode([
                "player_IDs" => $playerIds
            ], JSON_PRETTY_PRINT);

            $new_id = GetUniqueID2();

            $final_sql = "INSERT INTO session (session_id, player_ids, level_name, is_full) VALUES ('$new_id', '$outputJSON', 'CCE Experiment', $is_full);";
            
            PostToDatabase($final_sql);
        }
    }
}

function append_session(){
    $_t = reduceMinutesToTime(GetCurrentTime(), 25);
    $a = PostAndGetFromDatabase("SELECT GROUP_CONCAT(participant_id) AS participant_ids FROM participant WHERE condition_part_1 = 2 AND condition_part_2 = 0 and CAST(time_joined as time) > '$_t';");
    $b = PostAndGetFromDatabase('SELECT session_id, player_IDs FROM session WHERE is_full = 0 ORDER BY ID DESC LIMIT 1');
    var_dump($a);
    if ($b != null && $a != null){
        $temp_session = $b[0]['session_id'];
        $stringJSON = $b[0]['player_IDs'];
        $existingData = json_decode($stringJSON, true);
        $exploded = explode(",", $a[0]["participant_ids"]);
        foreach ($exploded as $_id){
            if (!in_array($_id, $existingData['player_IDs'])){
                $existingData['player_IDs'][] = $_id;
            }
            PostToDatabase("UPDATE participant SET condition_part_2 = 1 WHERE participant_id = '$_id';");
        }
        $updatedJsonString = json_encode($existingData);

        if (count($existingData['player_IDs']) == 5){
            $updateQuery = "UPDATE session SET player_IDs = '$updatedJsonString', is_full=1 WHERE session_id='$temp_session'";
        }
        else{
            $updateQuery = "UPDATE session SET player_IDs = '$updatedJsonString' WHERE session_id='$temp_session'";
        }

        PostToDatabase($updateQuery);
        return True;
    }
    return False;
}

// Connecting to the database.
try {
    $dbh = new PDO("mysql:host=$hostname;dbname=$database", $username, $password);
    $dbh->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch(PDOException $e) {
    echo '<h1>Connection error.</h1><pre>', $e->getMessage() ,'</pre>';
}

function get_min($array){
    $min = 1000000;
    $min_index = 0;
    $index = 0;
    foreach ($array as $a){
        if (intval($a) <= $min){
            $min = intval($a);
            $min_index = $index;
        }
        $index++;
    }

    return $min_index;
}

$all_n_requirements = [1, 2, 5];
$all_cons = [0, 1, 2];
$all_cons_count = [];
$min_index = -1;
$cur_index = 0;
$count_group = PostAndGetFromDatabase("SELECT `condition_part_1` as con, COUNT(*) as count, `participant_id` as pid FROM participant GROUP BY condition_part_1 ORDER BY condition_part_1 ASC");

$min_index = intval($given_con);
$sql = "INSERT INTO participant (participant_id, consent, age_years, gender, is_human, condition_part_1) VALUES ('$participant_id', $consent, $age, '$gender', $is_human, $min_index);";
logger();
PostToDatabase($sql);

if ($min_index == 2){
    $_t = reduceMinutesToTime(GetCurrentTime(), 25);
    $a = PostAndGetFromDatabase("SELECT COUNT(*) AS count 
    FROM participant 
    WHERE condition_part_2 = 0 
      AND condition_part_1 = 2 
      AND CAST(time_joined AS TIME) > '$_t';");
    $is_session = PostAndGetFromDatabase("SELECT Count(*) as count FROM session WHERE is_full=0");
    if ($a != null && $a[0]['count'] == 2){
        add_session(2, 0);
    }
    else if ($a != null && ($a[0]['count'] > 2 || $is_session[0]['count'] == 1)){
        $is_posted = append_session();
    }
}   
else{
    $count_group = PostAndGetFromDatabase("SELECT `condition_part_1` as con, COUNT(*) as count, `participant_id` as pid FROM participant GROUP BY condition_part_1 ORDER BY condition_part_1 ASC");
    if ($count_group[$min_index]["count"] % $all_n_requirements[$min_index] == 0){
        if ($min_index == 2){
            add_session($min_index, 0);
        }
        else{
            add_session($min_index, 1);
        }
    }
}


?>
