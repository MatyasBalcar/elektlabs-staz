DELETE FROM BOOKSAUTHORS;
DELETE FROM BOOKS;
DELETE FROM AUTHORS;
DELETE FROM PUBLISHERS;
DELETE FROM LANGUAGES;
DELETE FROM NATIONALITIES;

INSERT INTO Nationalities (Name) VALUES ('Česká republika');
INSERT INTO Nationalities (Name) VALUES ('Švédsko');
INSERT INTO Nationalities (Name) VALUES ('Velká Británie');
INSERT INTO Nationalities (Name) VALUES ('USA');
INSERT INTO Nationalities (Name) VALUES ('Francie');
INSERT INTO Nationalities (Name) VALUES ('Polsko');

INSERT INTO Languages (Name) VALUES ('Čeština');
INSERT INTO Languages (Name) VALUES ('Švédština');
INSERT INTO Languages (Name) VALUES ('Angličtina');
INSERT INTO Languages (Name) VALUES ('Francouzština');
INSERT INTO Languages (Name) VALUES ('Polština');

INSERT INTO Publishers (Name) VALUES ('Albatros');
INSERT INTO Publishers (Name) VALUES ('Mladá fronta');
INSERT INTO Publishers (Name) VALUES ('Host');
INSERT INTO Publishers (Name) VALUES ('Argo');
INSERT INTO Publishers (Name) VALUES ('Bloomsbury');
INSERT INTO Publishers (Name) VALUES ('Penguin Books');

INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('Karel', 'Čapek', '1890-01-09', 1);
INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('Astrid', 'Lindgrenová', '1907-11-14', 2);
INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('Jaroslav', 'Foglar', '1907-07-06', 1); 
INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('J. K.', 'Rowlingová', '1965-07-31', 3);
INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('George', 'Orwell', '1903-06-25', 3); 
INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('Ernest', 'Hemingway', '1899-07-21', 4);
INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('Andrzej', 'Sapkowski', '1948-06-21', 6);
INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) VALUES ('Victor', 'Hugo', '1802-02-26', 5); 

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Děti z Bullerbynu', '1947-01-01', 1, 1, 'Klasické vyprávění o šesti dětech z malé švédské vesnice.', TRUE, 5, '9788000075143');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('R.U.R.', '1920-01-01', 2, 1, 'Vědeckofantastické drama, které dalo světu slovo ROBOT.', TRUE, 4, '9788000000001');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Stínadla se bouří', '1947-01-01', 1, 1, 'Druhý díl dobrodružství Rychlých šípů v tajemných Stínadlech.', FALSE, 5, '9788000000002');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Harry Potter a Kámen mudrců', '1997-06-26', 5, 3, 'První díl slavné ságy o mladém čaroději.', TRUE, 5, '9780747532699');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('1984', '1949-06-08', 6, 3, 'Dystopický román o totalitním režimu a Velkém bratru.', TRUE, 5, '9780451524935');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Stařec a moře', '1952-09-01', 4, 3, 'Příběh o kubánském rybáři a jeho boji s obrovským marlínem.', FALSE, 3, '9780684801223');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Zaklínač I: Poslední přání', '1993-01-01', 3, 5, 'Soubor povídek o vědmákovi Geraltovi z Rivie.', TRUE, 4, '9788025707500');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Bídníci', '1862-01-01', 4, 4, 'Monumentální román o nespravedlnosti a vykoupení.', FALSE, 5, '9782253096337');

INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (1, 2); 
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (2, 1); 
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (3, 3); 
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (4, 4); 
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (5, 5);
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (6, 6);
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (7, 7); 
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (8, 8); 

COMMIT;