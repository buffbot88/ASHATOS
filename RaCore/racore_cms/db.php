<?php
// Database layer for RaCore CMS

class Database {
    private $db;
    
    public function __construct() {
        $this->db = new PDO('sqlite:' . DB_PATH);
        $this->db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    }
    
    public function query($sql, $params = []) {
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        return $stmt->fetchAll(PDO::FETCH_ASSOC);
    }
    
    public function execute($sql, $params = []) {
        $stmt = $this->db->prepare($sql);
        return $stmt->execute($params);
    }
}
?>