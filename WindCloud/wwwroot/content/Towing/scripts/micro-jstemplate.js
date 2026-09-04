var TemplateEngine = function (html, options) {
  if (typeof html != 'string') {
    html = $(html).html();
  }
  else if (html.indexOf('#') == 0) {
    html = $(html).html();
  }
  if (!html) return html;
  var re = /{{([^}}]+)?}}/g, reExp = /(^( )?(if|for|else|switch|case|break|{|}))(.*)?/g, code = 'var r=[];\n', cursor = 0, match;
  var add = function (line, js) {
    // console.log('TemplateEngine:add', line, js);
    js ? (code += line.match(reExp) ? line + '\n' : 'r.push(' + line + ');\n') :
      (code += line != '' ? 'r.push("' + line.replace(/"/g, '\\"').trim() + '");\n' : '');
    return add;
  }
  while (match = re.exec(html)) {
    add(html.slice(cursor, match.index))(match[1], true);
    cursor = match.index + match[0].length;
  }
  add(html.substr(cursor, html.length - cursor));
  code += 'return r.join("");';
  code = code.replace(/[\r\t\n]/g, '');
  window.debugTE = code;
  return new Function(code).apply(options);
}
/*
 * http://krasimirtsonev.com/blog/article/Javascript-template-engine-in-just-20-line
 **/