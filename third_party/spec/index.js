import { parse } from 'cddl'
import fs from 'node:fs/promises'

const ast = parse('./all.cddl')
await fs.writeFile('./webdriver-bidi.ast.json', JSON.stringify(ast, null, 2));
