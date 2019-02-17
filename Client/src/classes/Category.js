import buildPath from "services/buildPath"


export default class Category {
  getPath(me = true) {
    let tokens = [];
    let current = this.parent;
    while(current) {
      if(current.isMain) {
        tokens.push(current.name);
      }

      current = current.parent;
    }

    let newTokens = ["/"];
    for(let i = tokens.length-1; i>=0 ;i--) {
      newTokens.push(tokens[i]);
    }

    if(me)
      newTokens.push(this.name);

    return buildPath(...newTokens);
  }
}
