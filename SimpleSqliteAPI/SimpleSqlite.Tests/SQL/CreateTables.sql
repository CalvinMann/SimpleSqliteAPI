CREATE TABLE "Table1" (
	Id	INTEGER NOT NULL PRIMARY KEY,
	Value1	TEXT NOT NULL,
	Value2	REAL
);

CREATE TABLE "Table2" (
	Value3	TEXT NOT NULL,
	Value4	TEXT,
	PRIMARY KEY(Value3)
);

CREATE TABLE "Table3" (
	Value5	INTEGER NOT NULL,
	Value6	TEXT,
	Value7	BLOB NOT NULL,
	Value8	REAL,
	Value9	NUMERIC NOT NULL,
	PRIMARY KEY(Value5)
);

CREATE TABLE "Table4" (
	Id INTEGER NOT NULL PRIMARY KEY,
	Table1Id INTEGER NOT NULL,
	Value TEXT,
	FOREIGN KEY(Table1Id) REFERENCES Table1(Id)
);

INSERT INTO Table1 (Value1, Value2) VALUES ("Test", 15.5);
INSERT INTO Table1 (Value1, Value2) VALUES ("record 2", 8);
INSERT INTO Table1 (Value1, Value2) VALUES ("  ", NULL);

INSERT INTO Table2 (Value3, Value4) VALUES ("key", NULL);
INSERT INTO Table2 (Value3, Value4) VALUES ("key2", "value");

INSERT INTO Table3 (Value5, Value6, Value7, Value8, Value9) VALUES (15, "test", X'fd', NULL, 10.5);

INSERT INTO Table4 (Table1Id, Value) VALUES (1, "value1");
INSERT INTO Table4 (Table1Id, Value) VALUES (2, "value5");