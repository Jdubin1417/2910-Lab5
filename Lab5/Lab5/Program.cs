using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
}
class Author
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<Book> Books { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source=/Users/justindubin/Desktop/Lab5/Lab4.db";
        var books = GetBooks(connectionString);
        foreach (var book in books) Console.WriteLine($"Title: {RevertTitle(book.Title)}");
        WriteBooksToCsv(books, "books.csv");
        Console.Write("Enter the first letter of the last name: ");
        char letter = Console.ReadLine()[0];
        var authors = GetAuthorsByLetter(connectionString, letter);
        foreach (var author in authors) Console.WriteLine($"{author.LastName}, {author.FirstName}");

        // Get author ID from user and display books written by the author
        Console.Write("Enter the author ID to display their books: ");
        int authorId = int.Parse(Console.ReadLine());

        Author selectedAuthor = GetAuthorById(connectionString, authorId);
        if (selectedAuthor != null)
        {
            Console.WriteLine($"Books by {selectedAuthor.FirstName} {selectedAuthor.LastName}:");
            var authorBooks = GetAuthorBooks(connectionString, authorId);
            foreach (var book in authorBooks)
            {
                Console.WriteLine($"- {RevertTitle(book.Title)}");
            }
        }
        else
        {
            Console.WriteLine($"No author found with ID {authorId}.");
        }
    }

    //Gets books from the table
    static List<Book> GetBooks(string connectionString)
    {
        var books = new List<Book>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand("SELECT Id, Title FROM Books ORDER BY Title", connection))
            using (var reader = command.ExecuteReader())
                while (reader.Read()) books.Add(new Book { Id = reader.GetInt32(0), Title = reader.GetString(1) });
        }
        return books;
    }

    //Puts "The" at the beginning of Titles instead of at the end
    static string RevertTitle(string title)
    {
        if (title.EndsWith(", The"))
        {
            return "The " + title.Substring(0, title.Length - 5);
        }
        return title;
    }

    //Ensured format was correct in CSV and Text
    static string EscapeCsvField(string text)
    {
        if (text.Contains(',') || text.Contains('"') || text.Contains('\n'))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        return text;
    }

    //Writes or updates CSV File
    static void WriteBooksToCsv(List<Book> books, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Id,Title");
            foreach (var book in books) writer.WriteLine($"{book.Id},{EscapeCsvField(RevertTitle(book.Title))}");
        }
    }

    //Gets authors by first letter of last name
   static List<Author> GetAuthorsByLetter(string connectionString, char letter)
    {
        var authors = new List<Author>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand("SELECT Id, First_Name, Last_Name FROM Authors WHERE Last_Name LIKE @letter ORDER BY Last_Name, First_Name", connection))
            {
                command.Parameters.AddWithValue("@letter", $"{letter}%");
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) authors.Add(new Author { Id = reader.GetInt32(0), FirstName = reader.GetString(1), LastName = reader.GetString(2) });
            }
        }
        return authors;
    } 

    //Gets author by ID to display books
    static Author GetAuthorById(string connectionString, int authorId)
    {
        Author author = null;
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand("SELECT Id, First_Name, Last_Name FROM Authors WHERE Id = @Author_Id", connection))
            {
                command.Parameters.AddWithValue("@Author_Id", authorId);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        author = new Author { Id = reader.GetInt32(0), FirstName = reader.GetString(1), LastName = reader.GetString(2) };
                    }
                }
            }
        }
        return author;
    }

    //Allows program to show the books from the authors name
    static List<Book> GetAuthorBooks(string connectionString, int authorId)
    {
        var books = new List<Book>();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = new SqliteCommand("SELECT Books.Id, Books.Title FROM Books INNER JOIN Book_Author ON Books.Id = Book_Author.Book_Id WHERE Book_Author.Author_Id = @Author_Id", connection))
            {
                command.Parameters.AddWithValue("@Author_Id", authorId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) books.Add(new Book { Id = reader.GetInt32(0), Title = reader.GetString(1) });
            }
        }
        return books;
    }
}