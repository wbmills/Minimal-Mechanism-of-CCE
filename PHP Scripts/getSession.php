<?php

$id = $_GET["participant_id"];
$hash = $_GET["hash"];



// check hash
$realHash = hash('SHA512', $id . $secretKey, false);
$realHash = strtoupper($realHash);
if ($hash != $realHash){
    exit ('Bad Hash :p');
}

// Connecting to the database.
try {
    $dbh = new PDO("mysql:host=$hostname;dbname=$database", $username, $password);
    $dbh->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch(PDOException $e) {
    echo '<h1>Connection error.</h1><pre>', $e->getMessage() ,'</pre>';
}

// post to SQL and get back information from table
function PostAndGetFromDatabase($sql) {
    global $hostname, $username, $password, $database;
    $dbh = new PDO("mysql:host=$hostname;dbname=$database", $username, $password);
    
    try {
        $statement = $dbh->query($sql);
        $statement->setFetchMode(PDO::FETCH_ASSOC);
        $result = $statement->fetchAll();
        return $result;
    } catch (PDOException $e) {
        throw new Exception('error: ' . $e->getMessage());
    }
}

function CheckSessions(){
    global $id;
    $sql = "SELECT player_IDs FROM session;";
    $sql2 = "SELECT session_id FROM session;";

    // Execute SQL queries
    $sessionIDs = PostAndGetFromDatabase($sql2);
    $all = PostAndGetFromDatabase($sql);
    $i=0;
    foreach ($all as $d) {
        $data = json_decode($d['player_IDs'], true);
        if ($data != null){
            foreach ($data['player_IDs'] as $a) {
                if ($a == $id) {
                    return $sessionIDs[$i]['session_id'];
                }
            }
        }
        $i += 1;
    }
    return "none";
}

function RecordPath(){
    global $x, $y, $z, $a, $id;
    // last record was more than a second, record, else: update
    $pathSQL = "SELECT * FROM path WHERE participant_id='$id' AND run_id='0';";
    $path = PostAndGetFromDatabase($pathSQL);
    if (count($path) == 0){
        $sql = "INSERT INTO path (participant_ID, session_ID, run_id, nr, x, y, z, alpha) VALUES ('$id', 'default', '0', 0, $x, $y, $z, $a);";
    }
    else{
        $sql = "UPDATE path 
        SET x = $x, y = $y, z = $z, alpha = $a 
        WHERE participant_id = '$id' AND run_id='0';";
    }
    PostAndGetFromDatabase($sql);
}

function GetPositions(){
    $sql = "SELECT 
        CONCAT(
            'x=[', GROUP_CONCAT(x SEPARATOR ','), ']&',
            'y=[', GROUP_CONCAT(y SEPARATOR ','), ']&',
            'z=[', GROUP_CONCAT(z SEPARATOR ','), ']&',
            'alpha=[', GROUP_CONCAT(alpha SEPARATOR ','), ']&',
            'id=[', GROUP_CONCAT(participant_id SEPARATOR ','), ']'
            ) AS concatenated_values
    FROM (
        SELECT 
            GROUP_CONCAT(x) AS x, 
            GROUP_CONCAT(y) AS y, 
            GROUP_CONCAT(z) AS z, 
            GROUP_CONCAT(alpha) AS alpha, 
            GROUP_CONCAT(participant_id) AS participant_id
        FROM path 
        WHERE run_id = '0' ) AS concatenated_values;";
    $pos = PostAndGetFromDatabase($sql);
    if (count($pos) > 0){
        return $pos[0]["concatenated_values"];
    }
    else{
        return '&x=[]&y=[]&z=[]&alpha=[]&id=[]';
    }
}

function GetCon1(){
    global $id;
    $sql = "SELECT condition_part_1 FROM participant WHERE participant_id = '$id'";
    $answer = PostAndGetFromDatabase($sql);
    if (count($answer) > 0){
        return $answer[0]["condition_part_1"];
    }
    else{
        return 99;
    }
}

$session = CheckSessions();
$con1 = GetCon1();

if (!isset($_GET['session'])) {
    $x = $_GET["x"];
    $y = $_GET["y"];
    $z = $_GET["z"];
    $a = $_GET["a"];
    $posx = GetPositions();
    if ($session == "none"){
        RecordPath();
        echo '&x=[]&y=[]&z=[]&alpha=[]&id=[]' . "&session=null&con1=" . $con1;
    }
    else{
        echo  '&x=[]&y=[]&z=[]&alpha=[]&id=[]' . "&session=" . $session . "&con1=" . $con1;
    }
}
else{
    echo "&session=" . $session . "&con1=" . $con1;
}




?>
