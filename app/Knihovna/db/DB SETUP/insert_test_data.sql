DELETE FROM BOOKSAUTHORS;
DELETE FROM BOOKS;
DELETE FROM AUTHORS;

DELETE FROM PUBLISHERS;
DELETE FROM LANGUAGES;
DELETE FROM NATIONALITIES;

INSERT INTO Nationalities (Name) VALUES ('Ceska republika');
INSERT INTO Nationalities (Name) VALUES ('Svedsko');

INSERT INTO Languages (Name) VALUES ('Cestina');
INSERT INTO Languages (Name) VALUES ('Svedstina');

INSERT INTO Publishers (Name) VALUES ('Skvele nakladatelstvi');
INSERT INTO Publishers (Name) VALUES ('Albatros');

INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) 
VALUES ('Karel', 'Capek', '1890-01-09', 1);

INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) 
VALUES ('Astrid', 'Lindgren', '1907-11-14', 2);

INSERT INTO Authors (FirstName, LastName, DateOfBirth, NationalityID) 
VALUES ('Jaroslav', 'Foglar', '1907-07-06', 1);

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Deti z Bullerbynu', '2024-12-03', 1, 1, 'Deti z Bullerbynu (svedsky Alla vi barn i Bullerbyn) je klasicka detska kniha...', TRUE, 4, '9788000075143');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('R.U.R.', '1920-01-01', 2, 1, 'Klasicke drama o robotech.', TRUE, 3, '9788000000001');

INSERT INTO Books (Name, PublishDate, PublisherID, LanguageID, Description, HaveRead, Rating, ISBN) 
VALUES ('Stinadla se bouri', '1947-01-01', 2, 1, 'Dobrodruzny roman pro mladez.', FALSE, 5, '9788000000002');

INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (1, 2);
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (2, 1);
INSERT INTO BooksAuthors (BookID, AuthorID) VALUES (3, 3);

COMMIT;