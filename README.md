# TinyPost

TinyPost is a small utility to convert Markdown documents into HTML and post the results to Internet endpoints. For example, [my blog][1] supports AtomPub to publish new blog entries. This works well with Window Live Writer to write and post blog entries. However, I wanted to start writing blog entries in Markdown but there were no Markdown editors that would post to an AtomPub endpoint. Enter TinyPost.

TinyPost supports a few document headers to provide metadata on the post. These headers are `key:value` based and must be placed at the top of the document. The following is a list of valid headers:

* `email` - specifies the author's email address
* `date` - indicates a time in the future (or past) when TinyPost will send the document to the endpoint. TinyPost will skip documents whose date is set to the future.
* `name` - specify the name of the author.
* `tags` - add a comma separated list of categories to the document.
* `title` - the title of the document. If the title is not explicitly specified then the first line of text in the document will be used as the title.

Here is an example document that will be posted in the future with a couple tags titled, "For the future"

    date: 2020-2-20T20:20
    tags: future, personal

    # For the future

    In the future, on February 20th, 2020 at 8:20 PM to be exact, this
    document will appear. If you see it one of two things is true:

    1. You are in the year 2020.
    2. There was a bug in TinyPost.

    Either way, this is a reasonable example what is possible with
    TinyPost. Keep coding, you know I am.

    virtually,
       Rob Mensching

 [1]: http://robmensching.com/blog/
