-- MySQL dump 10.13  Distrib 8.0.27, for Linux (x86_64)
--
-- Host: localhost    Database: aiape
-- ------------------------------------------------------
-- Server version	8.0.27-0ubuntu0.20.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `aiape`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `aiape` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `aiape`;

--
-- Table structure for table `Answers`
--

DROP TABLE IF EXISTS `Answers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Answers` (
  `AnswerId` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `QuestionId` int NOT NULL,
  `Content` longtext NOT NULL,
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifyTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`AnswerId`),
  KEY `IX_Answers_QuestionId` (`QuestionId`),
  KEY `IX_Answers_UserId` (`UserId`),
  CONSTRAINT `FK_Answers_Questions_QuestionId` FOREIGN KEY (`QuestionId`) REFERENCES `Questions` (`QuestionId`) ON DELETE CASCADE,
  CONSTRAINT `FK_Answers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=15347 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `FavoriteAnswerRelations`
--

DROP TABLE IF EXISTS `FavoriteAnswerRelations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `FavoriteAnswerRelations` (
  `FavoriteId` int NOT NULL,
  `AnswerId` int NOT NULL,
  PRIMARY KEY (`FavoriteId`,`AnswerId`),
  KEY `IX_FavoriteAnswerRelations_AnswerId` (`AnswerId`),
  CONSTRAINT `FK_FavoriteAnswerRelations_Answers_AnswerId` FOREIGN KEY (`AnswerId`) REFERENCES `Answers` (`AnswerId`) ON DELETE CASCADE,
  CONSTRAINT `FK_FavoriteAnswerRelations_Favorites_FavoriteId` FOREIGN KEY (`FavoriteId`) REFERENCES `Favorites` (`FavoriteId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `FavoriteQuestionRelations`
--

DROP TABLE IF EXISTS `FavoriteQuestionRelations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `FavoriteQuestionRelations` (
  `FavoriteId` int NOT NULL,
  `QuestionId` int NOT NULL,
  PRIMARY KEY (`FavoriteId`,`QuestionId`),
  KEY `IX_FavoriteQuestionRelations_QuestionId` (`QuestionId`),
  CONSTRAINT `FK_FavoriteQuestionRelations_Favorites_FavoriteId` FOREIGN KEY (`FavoriteId`) REFERENCES `Favorites` (`FavoriteId`) ON DELETE CASCADE,
  CONSTRAINT `FK_FavoriteQuestionRelations_Questions_QuestionId` FOREIGN KEY (`QuestionId`) REFERENCES `Questions` (`QuestionId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Favorites`
--

DROP TABLE IF EXISTS `Favorites`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Favorites` (
  `FavoriteId` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `Name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` longtext NOT NULL,
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`FavoriteId`),
  KEY `IX_Favorites_UserId` (`UserId`),
  CONSTRAINT `FK_Favorites_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LikeAnswers`
--

DROP TABLE IF EXISTS `LikeAnswers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `LikeAnswers` (
  `UserId` int NOT NULL,
  `AnswerId` int NOT NULL,
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`AnswerId`,`UserId`),
  KEY `IX_LikeAnswers_UserId` (`UserId`),
  CONSTRAINT `FK_LikeAnswers_Answers_AnswerId` FOREIGN KEY (`AnswerId`) REFERENCES `Answers` (`AnswerId`) ON DELETE CASCADE,
  CONSTRAINT `FK_LikeAnswers_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LikeQuestions`
--

DROP TABLE IF EXISTS `LikeQuestions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `LikeQuestions` (
  `UserId` int NOT NULL,
  `QuestionId` int NOT NULL,
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`QuestionId`,`UserId`),
  KEY `IX_LikeQuestions_UserId` (`UserId`),
  CONSTRAINT `FK_LikeQuestions_Questions_QuestionId` FOREIGN KEY (`QuestionId`) REFERENCES `Questions` (`QuestionId`) ON DELETE CASCADE,
  CONSTRAINT `FK_LikeQuestions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NatrualAnswers`
--

DROP TABLE IF EXISTS `NatrualAnswers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `NatrualAnswers` (
  `NatrualAnswerId` int NOT NULL AUTO_INCREMENT,
  `Content` longtext NOT NULL,
  PRIMARY KEY (`NatrualAnswerId`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NatrualQuestionAnswerRelations`
--

DROP TABLE IF EXISTS `NatrualQuestionAnswerRelations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `NatrualQuestionAnswerRelations` (
  `NaturalQuestionId` int NOT NULL,
  `NatrualAnswerId` int NOT NULL,
  PRIMARY KEY (`NaturalQuestionId`,`NatrualAnswerId`),
  KEY `IX_NatrualQuestionAnswerRelations_NatrualAnswerId` (`NatrualAnswerId`),
  CONSTRAINT `FK_NatrualQuestionAnswerRelations_NatrualAnswers_NatrualAnswerId` FOREIGN KEY (`NatrualAnswerId`) REFERENCES `NatrualAnswers` (`NatrualAnswerId`) ON DELETE CASCADE,
  CONSTRAINT `FK_NatrualQuestionAnswerRelations_NatrualQuestions_NaturalQuest~` FOREIGN KEY (`NaturalQuestionId`) REFERENCES `NatrualQuestions` (`NaturalQuestionId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NatrualQuestions`
--

DROP TABLE IF EXISTS `NatrualQuestions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `NatrualQuestions` (
  `NaturalQuestionId` int NOT NULL AUTO_INCREMENT,
  `Content` longtext NOT NULL,
  PRIMARY KEY (`NaturalQuestionId`)
) ENGINE=InnoDB AUTO_INCREMENT=93 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `QuestionHotDatas`
--

DROP TABLE IF EXISTS `QuestionHotDatas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `QuestionHotDatas` (
  `QuestionId` int NOT NULL,
  `HotValue` int NOT NULL,
  `ModifyTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`QuestionId`),
  CONSTRAINT `FK_QuestionHotDatas_Questions_QuestionId` FOREIGN KEY (`QuestionId`) REFERENCES `Questions` (`QuestionId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `QuestionTagRelations`
--

DROP TABLE IF EXISTS `QuestionTagRelations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `QuestionTagRelations` (
  `TagId` int NOT NULL,
  `QuestionId` int NOT NULL,
  PRIMARY KEY (`QuestionId`,`TagId`),
  KEY `IX_QuestionTagRelations_TagId` (`TagId`),
  CONSTRAINT `FK_QuestionTagRelations_Questions_QuestionId` FOREIGN KEY (`QuestionId`) REFERENCES `Questions` (`QuestionId`) ON DELETE CASCADE,
  CONSTRAINT `FK_QuestionTagRelations_Tags_TagId` FOREIGN KEY (`TagId`) REFERENCES `Tags` (`TagId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Questions`
--

DROP TABLE IF EXISTS `Questions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Questions` (
  `QuestionId` int NOT NULL AUTO_INCREMENT,
  `BestAnswerId` int DEFAULT NULL,
  `UserId` int DEFAULT NULL,
  `Title` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Remarks` longtext NOT NULL,
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ModifyTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`QuestionId`),
  KEY `IX_Questions_UserId` (`UserId`),
  CONSTRAINT `FK_Questions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=15362 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Tags`
--

DROP TABLE IF EXISTS `Tags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Tags` (
  `TagId` int NOT NULL AUTO_INCREMENT,
  `Category` int NOT NULL,
  `Name` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Desc` text NOT NULL,
  PRIMARY KEY (`TagId`),
  UNIQUE KEY `Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `UserId` int NOT NULL AUTO_INCREMENT,
  `Email` varchar(254) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Bcrypt` char(60) NOT NULL,
  `Name` varchar(18) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Auth` int NOT NULL,
  `ProfilePhoto` int NOT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Name` (`Name`),
  UNIQUE KEY `AK_Users_Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-12-28 10:48:26
