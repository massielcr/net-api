import { createServer } from 'node:http'
import { createYoga, createSchema } from 'graphql-yoga'
import { createPubSub } from 'graphql-yoga'
import { pies } from './data.js'

const pubSub = createPubSub();

//Schema
const typeDefs = `
  type Pie {
    id: ID!
    name: String!
    price: Float!
    inStock: Boolean!
    averageRating: Float!
    category: String!
  }

  type Pies {
    pies: [Pie!]!
    count: Int!
  }

  type Query {
    pies: [Pie!]
    getPiesByCategory(category: String): Pies!
  }

  type Mutation {
    addPie(input: PieInput!): Pie!
  }

  input PieInput {
    name: String!
    price: Float!
    inStock: Boolean!
    averageRating: Float
    category: String
  }

  type Subscription {
    pieAdded: Pie!
  }
`;





const  resolvers = {
  Query: {
    pies: () => pies,
    getPiesByCategory: (parent, { category }) => {      
       const filteredPies = category ? pies.filter(pie => pie.category === category) : pies;
       return {
          pies: filteredPies,
          count: filteredPies.length
       };
    }
  },
  Mutation: {
    addPie: (parent, { input }) => {
      const newPie = {
        id: pies.length + 1,
        ...input,
      };

      pies.push(newPie);
      pubSub.publish('pieAdded', { pieAdded: newPie }); // Publish the new pie to subscribers

      return newPie;
    },
  },
  Subscription: {
    pieAdded: {
      subscribe: () => pubSub.subscribe('pieAdded')
    }
  }
};



const yoga = createYoga({
  schema: createSchema({
    typeDefs,
    resolvers,
  }),
  graphqlEndpoint: '/graphql',
})


const server = createServer(yoga)

server.listen(4000, () => {
  console.info('Server is running on http://localhost:4000/graphql')
})
