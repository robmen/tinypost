# tinyPost

tinyPost is a small utility to convert Markdown documents into HTML and post the results to Internet endpoints. For example, [my blog][1] supports AtomPub to publish new blog entries. This works well with Window Live Writer to write and post blog entries. However, I wanted to start writing blog entries in Markdown but there were no Markdown editors that would post to an AtomPub endpoint. Enter tinyPost.

## Document format

tinyPost supports a few document headers to provide metadata on the post. These headers are `key:value` based and must be placed at the top of the document. The following is a list of valid headers:

* `email` - specifies the author's email address
* `date` - indicates a time in the future (or past) when tinyPost will send the document to the endpoint. tinyPost will skip documents whose date is set to the future.
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
    2. There was a bug in tinyPost.

    Either way, this is a reasonable example what is possible with
    tinyPost. Keep coding, you know I am.

    virtually,
       Rob Mensching

[1]: http://robmensching.com/blog/


## Command line

tinyPost command line support posting Markdown documents to two different endpoints:

1. AtomPub via the `atom` command line action.
2. Zendesk via the `zendesk` command line action.

Each action supports the `-u` command line switch to specify the user name to access the end point and the `'-p` command line switch to provide the password.

### Atom action

The `atom` command line action must be provided the URI to the AtomPub endpoint. For example:

    tinypost atom http://example.com/admin/atompub

Would post unauthenticated to the AtomPub endpoint at `http://example.com/admin/atompub`. As noted above, the `-u` and `-p` switches could be used to provide a user name/password.

### Zendesk action

The `zendesk` command line action must be provided the sub-domain for the support desk hosted at Zendesk.com. For example:

    tinypost zendesk -u robmen -p pa$$word example

Would post to the `example.zendesk.com` support desk using the username `robmen` and password `pa$$word`.

It is important to note that the `zendesk` action posts new articles to a forum named `Staged`. That forum must exist in the support desk before articles can be posted. It is recommended that the `Staged` forum be visible only to agents so that new content can be reviewed then categorized in the appropriate forum.

Also note, a post that are edited then posted again will update the existing article in the support desk without using the `Staged` forum.
