<?php
// get arguements from URL
$id = $_GET["participant_id"];
$session_id = $_GET["session_id"];
$hash = $_GET["hash"];
$x = $_GET["x"];
$y = $_GET["y"];
$z = $_GET["z"];
$a = $_GET["a"];

// set variables for paired run
$max_nr = 6;
$max_gens = 2;
$max_nr_overall = 12;
$curRunInfo = null;
$curNr;
$timeToWait = 10;
$curRunID = null;



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

// if all gerations are completed, evaluated by getting the total number of runs from all generations
function GensCompleted(){
    global $session_id;
    $sql = "SELECT COUNT(nr) AS total_nr
    FROM run
    WHERE session_id = '$session_id' AND nr=6;";
    $result = PostAndGetFromDatabase($sql);
    if (count($result)>0){
        $result = $result[0]["total_nr"];
    }
    else{
        $result = 0;
    }
    return $result;
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
        return null;
        // Error occurred during erunData$runDataecution
        //throw new Exception('error: ' . $e->getMessage());
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
        return null;
        //throw new Exception('Error: ' . $e->getMessage());
    }
}

// get run info using run id, if no id then create new run
function GetRunData(){
    global $session_id, $dbh, $curNr, $max_nr, $curRunID;
    $sql = "SELECT *
    FROM run 
    WHERE session_ID = '$session_id' 
    ORDER BY start_time DESC
    LIMIT 1;";
    $runData = PostAndGetFromDatabase($sql);
    // if no run ID in session or last run ID nr == max runs, create new run
    if (count($runData) == 0 || $runData == null || $runData[0]["nr"] >= $max_nr){ 
        $curRunID = GetUniqueID2();
        $t1 = GetCurrentTime();
        $t2 = addMinutesToTime($t1, 3);
        try{
            PostToDatabase("ALTER TABLE run AUTO_INCREMENT = 1;");
            PostToDatabase("INSERT INTO run (run_id, session_ID, is_finished, start_time, goal_time, nr) VALUES 
            ('$curRunID', '$session_id', 0, '$t1', '$t2', 0);");
        }
        catch (Exception $e){
            echo("ERROR: " . $e);
        }
        $runData = PostAndGetFromDatabase($sql);
    }
    $curNr = $runData[0]["nr"] + 1;
    return $runData[0];
}

// record position and run info of current participant and post to database
function RecordPath(){
    global $curNr, $x, $y, $z, $a, $session_id, $id, $curRunInfo, $curRunID;
    $rid = $curRunInfo["run_id"];
    // last record was more than a second, record, else: update
    $sqlTime = "SELECT t FROM path WHERE participant_id='$id' ORDER BY t DESC LIMIT 1;";

    $lastTime = PostAndGetFromDatabase($sqlTime);
    $currentTime = GetCurrentTime();

    //var_dump($lastTime[0]);
    if (count($lastTime) == 0 || $lastTime[0]["t"] == null || GetTimeDifferenceSeconds($lastTime[0]['t'], $currentTime) >= 0.1){
        //$t = DateTime::createFromFormat('Y-m-d H:i:s.u', $currentTime);
        $sql = "INSERT INTO path (participant_ID, session_ID, run_id, nr, x, y, z, alpha, t) VALUES (
            '$id', '$session_id', '$rid', $curNr, $x, $y, $z, $a, '$currentTime');";
    }
    else{
        $sql = "UPDATE path 
        SET x = $x, y = $y, z = $z, alpha = $a, nr = $curNr 
        WHERE participant_id = '$id' 
        AND t = (SELECT MAX(t) FROM path WHERE participant_id = '$id');";
    }
    PostToDatabase($sql);
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

// get whether participant is within goal with radius of 50 units
function atGoal($x1, $z1, $goalX, $goalZ){
    if ($x1 >= $goalX - 20 && 
    $x1  <= $goalX + 20 &&
    $z1 >= $goalZ - 20 &&
    $z1 <= $goalZ + 20){
        return true;
    }
    else{
        return false;
    }
}

// if nr > 0 (they have begun) or nr < 12 (they have finished) then return their id's
function who_is_playing(){
    global $session_id, $max_nr_overall, $max_nr;#
    $gens = GensCompleted();
    // get all ids in session
    $sql = "SELECT player_IDs
    FROM session
    WHERE session_id = '$session_id';";
    $all_ids_json = PostAndGetFromDatabase($sql);
    // if no names, exit
    if ($all_ids_json == null || count($all_ids_json) == 0){
        return null;
    }
    else{
        $all_ids_json = $all_ids_json[0]['player_IDs'];
    }
    $data = json_decode($all_ids_json, true)['player_IDs'];
    // if on first gen, player = #1, else player = #2
    if ($gens == 0){
        $player = $data[0];
    }
    else if ($gens == 1){
        $player = $data[0];
    }
    else{
        $player = [];
    }

    return $player;
}

function addMinutesToTime($time, $minutesToAdd) {
    $dateTime = DateTime::createFromFormat('Y-m-d H:i:s.u', $time);
    $dateTime->modify("+{$minutesToAdd} minutes");
    return $dateTime->format('Y-m-d H:i:s.u');
}

function addSecondsToTime($time, $secondsToAdd) {
    $dateTime = DateTime::createFromFormat('Y-m-d H:i:s.u', $time);
    $dateTime->modify("+{$secondsToAdd} seconds");
    return $dateTime->format('Y-m-d H:i:s.u');
}

function GetTimeDifferenceMinutes($t1, $t2){
    $start_time = DateTime::createFromFormat('Y-m-d H:i:s.u', $t1);
    $goal_time = DateTime::createFromFormat('Y-m-d H:i:s.u', $t2);

    // Calculate the time difference between goal_time and start_time
    $time_difference = $goal_time->diff($start_time);
    return $time_difference->i;
}

function isTimeInPast($t1) {
    // Get the current time
    $currentTime = DateTime::createFromFormat('Y-m-d H:i:s.u', GetCurrentTime());

    // Create a DateTime object from $t1. First attempt to convert from
    // a string with microseconds. If that fails (because they might not
    // be there), try without microseconds.
    $providedTime = DateTime::createFromFormat('Y-m-d H:i:s.u', $t1);
    if ($providedTime == null) {
        $providedTime = DateTime::createFromFormat('Y-m-d H:i:s', $t1);
    }

    // Check if the provided time is in the past
    return $providedTime < $currentTime;
}

function GetTimeDifferenceSeconds($t1, $t2){
    $start_time = DateTime::createFromFormat('Y-m-d H:i:s.u', $t1);
    $goal_time = DateTime::createFromFormat('Y-m-d H:i:s.u', $t2);

    // Calculate the time difference between goal_time and start_time
    $time_difference = $goal_time->diff($start_time);
    return $time_difference->s;
}

function GetState() {
    global $id, $session_id, $dbh, $x, $y, $z, $max_gens, $max_nr, $curRunInfo, $timeToWait;

    // get goal position, current run data, and goal times
    $runInfo = $curRunInfo;
    $goal = PostAndGetFromDatabase("SELECT goal_x, goal_y, start_x, start_y FROM level WHERE name = 'CCE Experiment';")[0];
    $nr = (int)$runInfo["nr"];
    $goal_time = $runInfo["goal_time"];
    $start_time = $runInfo["start_time"];
    $completeRun = atGoal($x, $z, $goal["goal_x"], $goal["goal_y"]);
    $state = 0;
    $run_id_temp = $runInfo["run_id"];
    
    // if run f**ks up
    if($runInfo == null || count($runInfo) == 0){
        echo ("Run ID non-existant");
    }
    
    // reset = 0: nothing, reset = 1: time warning, reset = 2: wait for other player, reset = 3: back to start, reset = 4: finish trial.
    // if gen done
    if ($nr == $max_nr-1 && isTimeInPast($goal_time)){
        $state = 4;
        PostToDatabase("UPDATE run SET is_finished = 1, nr = $nr + 1 WHERE run_id = '$run_id_temp';");
        }
    // if goal time elapsed, go back to start
    else if (isTimeInPast($goal_time)){
        $state = 3;
        $c_time = GetCurrentTime();
        $c_goal = addMinutesToTime($c_time, 3); 
        PostToDatabase("UPDATE run SET nr = $nr + 1, start_time = '$c_time', goal_time = '$c_goal' WHERE run_id = '$run_id_temp';");
    }
    // if run complete but time has not elapsed
    else if ($completeRun) {
        // set goal_time to current time + 30 seconds
        $state = 2;
        // if time has not been changed yet AND there are more than 30 seconds left in run
        if (addSecondsToTime(GetCurrentTime(), $timeToWait) < $goal_time){
            $new_time = addSecondsToTime(GetCurrentTime(), $timeToWait);
            PostToDatabase("UPDATE run SET goal_time = '$new_time';");
        }
    }
    // if in final 30 seconds, show warning
    else if (addSecondsToTime(GetCurrentTime(), 10) >= $goal_time){
        $state = 1;
    }
    // else, just keep going
    else{
        $state = 0;
    }
    RecordPath();
    return "&state=" . $dbh->quote($state) . "&start_x=" . $dbh->quote($goal["start_x"]). "&start_z=" . $dbh->quote($goal["start_y"]);
}

function GetCurrentTime(){
    $microtime = microtime(true);
    $milliseconds = round($microtime * 1000);
    $dateTime = DateTime::createFromFormat('U.u', $microtime);
    return $dateTime->format('Y-m-d H:i:s.u');
}

if (GensCompleted() == $max_gens){
    // return the state of the game with no player positions (single trial does not require this)
    $final = "myTurn=false&x=[]&y=[]&z=[]&alpha=[]&id=[]&state=4&nr=6&gens=" . GensCompleted();
}
else{
    $curRunInfo = GetRunData();
    // return the state of the game with no player positions (single trial does not require this)
    $final = "myTurn=true&x=[]&y=[]&z=[]&alpha=[]&id=[]" . GetState() . "&nr=" . $curRunInfo["nr"] . "&gens=" . GensCompleted();
}
echo $final;


?>
