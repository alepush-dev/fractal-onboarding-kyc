DELIMITER $$
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_InsertOnboardingRecord`(
    IN p_Email VARCHAR(150),
    IN p_FullName VARCHAR(250),
    IN p_DocumentNumber VARCHAR(50),
    IN p_OcrConfidence DECIMAL(5,2),
    IN p_ImageUrl TEXT
)
BEGIN
    INSERT INTO ClientsKyc (Email, FullName, DocumentNumber, OcrConfidence, ImageUrl, CreatedAt)
    VALUES (p_Email, p_FullName, p_DocumentNumber, p_OcrConfidence, p_ImageUrl, NOW());
    
    SELECT LAST_INSERT_ID() AS NewId;
END$$
DELIMITER ;
