import os
import numpy as np
from flask import Flask, redirect, url_for, request, render_template

app = Flask('Demo' ,root_path=os.path.dirname(__file__))

@app.route('/')
def hello_world():
    return redirect(url_for('new_root1'))
#     return '''<html>
# <h1>Hello World</h1>
# </html>
# '''

@app.route('/teststr/<name>')
def hello_str(name):
    return '''<html>
    <body>
        <h1> Hello {}
    </body>
</html>
'''.format(name)

@app.route('/testint/<int:i>')
def hello_int(i):
    return '''<html>
    <body>
        <h1> Hello {}
    </body>
</html>
'''.format(i)

@app.route('/test1')
def test1():
    return 'Test1'

'''
This function will never be executed.
'''
@app.route('/test2')
def test2f():
    return 'Test2first'

@app.route('/test2/')
def test2s():
    return 'Test2second'

@app.route('/admin')
def admin():
    return 'Hello admin'

@app.route('/guest/<name>')
def guest(name):
    return 'Hello guest {}'.format(name)

@app.route('/user/<name>')
def user(name):
    if name == 'admin':
        return redirect(url_for('admin'))
    else:
        return redirect(url_for('guest', name=name))

@app.route('/login_success/<name>')
def login_success(name):
    return 'Login success, {}'.format(name)

@app.route('/login')
def login():
    # print(type(request.args), ' | ', type(request.args['name']))
    return redirect(url_for('login_success', name=request.args['name']))

@app.route('/new_root/')
def new_root():
    my_int = 18
    my_str = 'curry'
    my_list = [1, 5, 4, 3, 2]
    my_dict = {
        'name': 'durant',
        'age': 28
    }
    return render_template('hello.html',
                           my_int=my_int,
                           my_str=my_str,
                           my_list=my_list,
                           my_dict=my_dict)

@app.route('/new_root1/')
def new_root1():
    return render_template('index.html')

@app.route('/test_json/')
def test_json():
    return {'a' :1, 'b': [1, 2, 3]}

@app.route('/test_post/', methods=['POST'])
def test_post():
    print(type(request.json))
    return request.json

@app.route('/test_numpy')
def test_numpy():
    return {'numpy_content': list(np.random.rand(2))}

if __name__ == '__main__':
    app.run()
