<?php

// Get variables from URL
$participant_id = $_GET["participant_id"]; // random string
$hash = $_GET["hash"]; // hash code



$realHash = hash('SHA512', $participant_id . $secretKey, false);
$realHash = strtoupper($realHash);
if ($hash != $realHash){
    exit("'Non-Matching Hash Code >:('");
}

// Connecting to the database.
try {
    $dbh = new PDO("mysql:host=$hostname;dbname=$database", $username, $password);
    $dbh->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch(PDOException $e) {
    echo '<h1>Connection error.</h1><pre>', $e->getMessage() ,'</pre>';
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

PostToDatabase("UPDATE participant SET condition_part_1 = 0 WHERE participant_id = '$participant_id';");

$all_ids = PostAndGetFromDatabase("SELECT GROUP_CONCAT(participant_id) AS participant_ids FROM participant WHERE participant_id = '$participant_id';");
if (count($all_ids) > 0){
    foreach($all_ids as $i){
        // convert to json
        $playerIds = explode(",", $i["participant_ids"]);
        $playerIds = array_map('strval', $playerIds);
        $outputJSON = json_encode([
            "player_IDs" => $playerIds
        ], JSON_PRETTY_PRINT);

        $new_id = GetUniqueID2();
        PostToDatabase("INSERT INTO session (session_id, player_ids, level_name) VALUES ('$new_id', '$outputJSON', 'CCE Experiment');");
    }
}


?>
